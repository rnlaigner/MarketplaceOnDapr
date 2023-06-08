using System;
using Common.Entities;
using Common.Events;
using Dapr.Client;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PaymentMS.Infra;
using PaymentMS.Integration;
using PaymentMS.Models;
using PaymentMS.Repositories;

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
            /*
             * We assume the payment provider exposes an idempotency ID 
             * that guarantees exactly once payment processing even when 
             * a payment request is submitted more than once to them
             */
            PaymentIntent intent = await externalProvider.Create(new PaymentIntentCreateOptions()
            {
                Amount = paymentRequest.totalInvoice,
                Customer = paymentRequest.customer.CustomerId.ToString(),
                IdempotencyKey = paymentRequest.invoiceNumber,
                cardOptions = new()
                {
                    Number = paymentRequest.customer.CardNumber,
                    Cvc = paymentRequest.customer.CardSecurityNumber,
                    ExpMonth = paymentRequest.customer.CardExpiration, // TODO parse
                    ExpYear = paymentRequest.customer.CardExpiration
                }
            });

            using (var dbContextTransaction = dbContext.Database.BeginTransaction())
            {

                if (!intent.Status.Equals("succeeded"))
                {
                    var res = new PaymentFailed(intent.Status, paymentRequest.customer, paymentRequest.orderId,
                        paymentRequest.items, paymentRequest.totalInvoice, paymentRequest.instanceId);
                    await this.daprClient.PublishEventAsync(PUBSUB_NAME, nameof(PaymentFailed), res);
                    this.logger.LogInformation("[ProcessPayment] failed: {0}.", paymentRequest.instanceId);

                    //dbContextTransaction.Commit();
                    return;
                }

                int seq = 1;

                var cc = paymentRequest.customer.PaymentType.Equals(PaymentType.CREDIT_CARD.ToString());
                // create payment tuples
                if (cc || paymentRequest.customer.PaymentType.Equals(PaymentType.DEBIT_CARD.ToString()))
                {
                    var cardPaymentLine = new OrderPaymentModel()
                    {
                        order_id = paymentRequest.orderId,
                        payment_sequential = seq,
                        payment_type = cc ? PaymentType.CREDIT_CARD : PaymentType.DEBIT_CARD,
                        payment_installments = paymentRequest.customer.Installments,
                        payment_value = paymentRequest.totalInvoice
                    };

                    // create an entity for credit card payment details with FK to order payment
                    OrderPaymentCardModel card = new()
                    {
                        order_id = paymentRequest.orderId,
                        payment_sequential = seq,
                        card_number = paymentRequest.customer.CardNumber,
                        card_holder_name = paymentRequest.customer.CardHolderName,
                        card_expiration = DateTime.Parse(paymentRequest.customer.CardExpiration),
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
                        payment_sequential = seq,
                        payment_type = PaymentType.BOLETO,
                        payment_installments = 1,
                        payment_value = paymentRequest.totalInvoice
                    });
                    seq++;
                }

                // then one line for each voucher
                foreach(var item in paymentRequest.items)
                {
                    foreach(var voucher in item.vouchers)
                    {
                        paymentLines.Add(new OrderPaymentModel()
                        {
                            order_id = paymentRequest.orderId,
                            payment_sequential = seq,
                            payment_type = PaymentType.VOUCHER,
                            payment_installments = 1,
                            payment_value = voucher
                        });
                        seq++;
                    }
                }

                if (paymentLines.Count() > 0)
                    dbContext.OrderPayments.AddRange(paymentLines);
                dbContext.SaveChanges();

                if (config.PaymentStreaming)
                {
                    var paymentRes = new PaymentConfirmed(paymentRequest.customer, paymentRequest.orderId, paymentRequest.totalInvoice, paymentRequest.items, DateTime.Today, paymentRequest.instanceId);
                    await this.daprClient.PublishEventAsync(PUBSUB_NAME, nameof(PaymentConfirmed), paymentRes);
                }
                this.logger.LogInformation("[ProcessPayment] confirmed: {0}.", paymentRequest.instanceId);

                dbContextTransaction.Commit();
            
            }

        }
    }
}

