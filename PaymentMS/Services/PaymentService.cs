using System;
using Common.Entities;
using Common.Events;
using Dapr.Client;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PaymentMS.Infra;
using Common.Integration;
using PaymentMS.Models;
using PaymentMS.Repositories;
using System.Globalization;
using System.Buffers.Text;

namespace PaymentMS.Services
{
	public class PaymentService : IPaymentService
	{

        private const string PUBSUB_NAME = "pubsub";

        private readonly PaymentDbContext dbContext;
        private readonly DaprClient daprClient;
        private readonly PaymentConfig config;
        private readonly IExternalProvider externalProvider;
        private readonly ILogger<PaymentService> logger;

        public PaymentService(PaymentDbContext dbContext, DaprClient daprClient, IOptions<PaymentConfig> config,
            IExternalProvider externalProvider, ILogger<PaymentService> logger)
		{
            this.dbContext = dbContext;
            this.daprClient = daprClient;
            this.config = config.Value;
            this.externalProvider = externalProvider;
            this.logger = logger;
		}

        /**
         * The invoice issued event can arrive more than once, but the customer is only charged once
         * Concurrent invoice issues will lead to corrupted state due to the constraints
         */
        public async Task ProcessPayment(InvoiceIssued paymentRequest)
        {

            this.logger.LogInformation("[ProcessPayment] started for order ID {0}.", paymentRequest.orderId);

            /*
             * We assume the payment provider exposes an idempotency ID 
             * that guarantees exactly once payment processing even when 
             * a payment request is submitted more than once to them
             */
            var now = DateTime.Now;

            // https://stackoverflow.com/questions/49727809
            var cardExpParsed = DateTime.ParseExact(paymentRequest.customer.CardExpiration, "MMyy", CultureInfo.InvariantCulture);

            var options = new PaymentIntentCreateOptions()
            {
                Amount = paymentRequest.totalInvoice,
                Customer = paymentRequest.customer.CustomerId.ToString(),
                IdempotencyKey = paymentRequest.invoiceNumber,
                cardOptions = new()
                {
                    Number = paymentRequest.customer.CardNumber,
                    Cvc = paymentRequest.customer.CardSecurityNumber,
                    ExpMonth = cardExpParsed.Month.ToString(),
                    ExpYear = cardExpParsed.Year.ToString()
                }
            };

            PaymentIntent? intent = await externalProvider.Create(options);

            if(intent is null)
            {
                throw new ApplicationException("[ProcessPayment] It was not possible to retrieve payment intent from external provider");
            }

            using (var txCtx = dbContext.Database.BeginTransaction())
            {

                // Based on: https://stripe.com/docs/payments/payment-intents/verifying-status
                PaymentStatus status = intent.status.Equals("succeeded") ? PaymentStatus.succeeded : PaymentStatus.requires_payment_method;
                
                int seq = 1;

                var cc = paymentRequest.customer.PaymentType.Equals(PaymentType.CREDIT_CARD.ToString());
                // create payment tuples
                if (cc || paymentRequest.customer.PaymentType.Equals(PaymentType.DEBIT_CARD.ToString()))
                {
                    var cardPaymentLine = new OrderPaymentModel()
                    {
                        order_id = paymentRequest.orderId,
                        sequential = seq,
                        type = cc ? PaymentType.CREDIT_CARD : PaymentType.DEBIT_CARD,
                        installments = paymentRequest.customer.Installments,
                        value = paymentRequest.totalInvoice,
                        status = status,
                        created_at = now
                    };

                    // create an entity for credit card payment details with FK to order payment
                    OrderPaymentCardModel card = new()
                    {
                        order_id = paymentRequest.orderId,
                        payment_sequential = seq,
                        card_number = paymentRequest.customer.CardNumber,
                        card_holder_name = paymentRequest.customer.CardHolderName,
                        card_expiration = cardExpParsed,
                        card_brand = paymentRequest.customer.CardBrand
                    };

                    dbContext.OrderPayments.Add(cardPaymentLine);
                    dbContext.SaveChanges();
                    dbContext.OrderPaymentCards.Add(card);

                    seq++;
                }

                List<OrderPaymentModel> paymentLines = new();
                if (paymentRequest.customer.PaymentType.Equals(PaymentType.BOLETO.ToString()))
                {
                    paymentLines.Add(new OrderPaymentModel()
                    {
                        order_id = paymentRequest.orderId,
                        sequential = seq,
                        type = PaymentType.BOLETO,
                        installments = 1,
                        value = paymentRequest.totalInvoice,
                        created_at = now,
                        status = status
                    });
                    seq++;
                }

                // vouchers apply only if payment is succeded
                if (intent.status.Equals("succeeded"))
                {
                    // then one line for each voucher
                    foreach (var item in paymentRequest.items)
                    {
                        foreach (var voucher in item.vouchers)
                        {
                            paymentLines.Add(new OrderPaymentModel()
                            {
                                order_id = paymentRequest.orderId,
                                sequential = seq,
                                type = PaymentType.VOUCHER,
                                installments = 1,
                                value = voucher,
                                created_at = now
                            });
                            seq++;
                        }
                    }
                }

                if (paymentLines.Count() > 0)
                    dbContext.OrderPayments.AddRange(paymentLines);
                dbContext.SaveChanges();

                if (config.PaymentStreaming)
                {

                    if (intent.status.Equals("succeeded"))
                    {
                        this.logger.LogInformation("[ProcessPayment] publishing payment confirmed event for order ID: {0}.", paymentRequest.orderId);
                        var paymentRes = new PaymentConfirmed(paymentRequest.customer, paymentRequest.orderId,
                            paymentRequest.totalInvoice, paymentRequest.items, now, paymentRequest.instanceId);
                        await this.daprClient.PublishEventAsync(PUBSUB_NAME, nameof(PaymentConfirmed), paymentRes);
                    }
                    else
                    {
                        this.logger.LogInformation("[ProcessPayment] publishing payment failed event for order ID: {0}.", paymentRequest.orderId);
                        var res = new PaymentFailed(intent.status, paymentRequest.customer, paymentRequest.orderId,
                               paymentRequest.items, paymentRequest.totalInvoice, paymentRequest.instanceId);
                        await this.daprClient.PublishEventAsync(PUBSUB_NAME, nameof(PaymentFailed), res);
                        this.logger.LogInformation("[ProcessPayment] failed for order ID: {0}.", paymentRequest.orderId);
                    }
                }
                this.logger.LogInformation("[ProcessPayment] confirmed for order ID {0}.", paymentRequest.orderId);

                txCtx.Commit();
            
            }

        }
    }
}

