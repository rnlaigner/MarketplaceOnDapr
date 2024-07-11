using Common.Entities;
using Common.Events;
using Dapr.Client;
using Microsoft.Extensions.Options;
using PaymentMS.Infra;
using Common.Integration;
using PaymentMS.Models;
using System.Globalization;
using Common.Driver;
using System.Text;
using PaymentMS.Repositories;

namespace PaymentMS.Services;

public class PaymentService : IPaymentService
{
    private const string PUBSUB_NAME = "pubsub";

    private readonly IPaymentRepository paymentRepository;
    private readonly DaprClient daprClient;
    private readonly PaymentConfig config;
    private readonly IExternalProvider externalProvider;
    private readonly ILogger<PaymentService> logger;

    public PaymentService(IPaymentRepository paymentRepository, DaprClient daprClient, IOptions<PaymentConfig> config,
        IExternalProvider externalProvider, ILogger<PaymentService> logger)
	{
        this.paymentRepository = paymentRepository;
        this.daprClient = daprClient;
        this.config = config.Value;
        this.externalProvider = externalProvider;
        this.logger = logger;
	}

    /**
     * The invoice issued event can arrive more than once, but the customer is only charged once
     * Concurrent invoice issues will lead to corrupted state due to the constraints
     */
    public async Task ProcessPayment(InvoiceIssued invoiceIssued)
    {
        // https://stackoverflow.com/questions/49727809
        var cardExpParsed = DateTime.ParseExact(invoiceIssued.customer.CardExpiration, "MMyy", CultureInfo.InvariantCulture);
        PaymentStatus status;
        if (config.PaymentProvider)
        { 
            /*
             * We assume the payment provider exposes an idempotency ID 
             * that guarantees exactly once payment processing even when 
             * a payment request is submitted more than once to them
             */
            var options = new PaymentIntentCreateOptions()
            {
                Amount = invoiceIssued.totalInvoice,
                Customer = invoiceIssued.customer.CustomerId.ToString(),
                IdempotencyKey = invoiceIssued.invoiceNumber,
                cardOptions = new()
                {
                    Number = invoiceIssued.customer.CardNumber,
                    Cvc = invoiceIssued.customer.CardSecurityNumber,
                    ExpMonth = cardExpParsed.Month.ToString(),
                    ExpYear = cardExpParsed.Year.ToString()
                }
            };

            PaymentIntent? intent = externalProvider.Create(options);

            if (intent is null)
            {
                throw new Exception("[ProcessPayment] It was not possible to retrieve payment intent from external provider");
            }
            // Based on: https://stripe.com/docs/payments/payment-intents/verifying-status
            status = intent.status.Equals("succeeded") ? PaymentStatus.succeeded : PaymentStatus.requires_payment_method;
        } else
        {
            status = PaymentStatus.succeeded;
        }

        var now = DateTime.UtcNow;
        using (var txCtx = this.paymentRepository.BeginTransaction())
        {
            int seq = 1;
            var cc = invoiceIssued.customer.PaymentType.Equals(PaymentType.CREDIT_CARD.ToString());
            // create payment tuples
            if (cc || invoiceIssued.customer.PaymentType.Equals(PaymentType.DEBIT_CARD.ToString()))
            {
                var cardPaymentLine = new OrderPaymentModel()
                {
                    customer_id = invoiceIssued.customer.CustomerId,
                    order_id = invoiceIssued.orderId,
                    sequential = seq,
                    type = cc ? PaymentType.CREDIT_CARD : PaymentType.DEBIT_CARD,
                    installments = invoiceIssued.customer.Installments,
                    value = invoiceIssued.totalInvoice,
                    status = status,
                    created_at = now
                };

                var entity = this.paymentRepository.Insert(cardPaymentLine);
                this.paymentRepository.FlushUpdates();

                // create an entity for credit card payment details with FK to order payment
                OrderPaymentCardModel card = new()
                {
                    customer_id = invoiceIssued.customer.CustomerId,
                    order_id = invoiceIssued.orderId,
                    sequential = seq,
                    card_number = invoiceIssued.customer.CardNumber,
                    card_holder_name = invoiceIssued.customer.CardHolderName,
                    card_expiration = cardExpParsed,
                    card_brand = invoiceIssued.customer.CardBrand,
                    orderPayment = entity
                };

                this.paymentRepository.Insert(card);
                seq++;
            }

            List<OrderPaymentModel> paymentLines = new();
            if (invoiceIssued.customer.PaymentType.Equals(PaymentType.BOLETO.ToString()))
            {
                paymentLines.Add(new OrderPaymentModel()
                {
                    customer_id = invoiceIssued.customer.CustomerId,
                    order_id = invoiceIssued.orderId,
                    sequential = seq,
                    type = PaymentType.BOLETO,
                    installments = 1,
                    value = invoiceIssued.totalInvoice,
                    created_at = now,
                    status = status
                });
                seq++;
            }

            // vouchers apply only if payment is succeded
            if (status == PaymentStatus.succeeded)
            {
                // then one line for each voucher
                foreach (var item in invoiceIssued.items)
                {
                    if(item.total_incentive > 0)
                    {
                        paymentLines.Add(new OrderPaymentModel()
                        {
                            customer_id = invoiceIssued.customer.CustomerId,
                            order_id = invoiceIssued.orderId,
                            sequential = seq,
                            type = PaymentType.VOUCHER,
                            installments = 1,
                            value = item.total_incentive,
                            created_at = now,
                            status = status
                        });
                        seq++;
                    }
                }
            }

            if (paymentLines.Count() > 0)
                this.paymentRepository.InsertAll(paymentLines);
            this.paymentRepository.FlushUpdates();
            txCtx.Commit();
            if (this.config.Streaming)
            {
                if (status == PaymentStatus.succeeded)
                {
                    var paymentRes = new PaymentConfirmed(invoiceIssued.customer, invoiceIssued.orderId,
                        invoiceIssued.totalInvoice, invoiceIssued.items, now, invoiceIssued.instanceId);
                    await this.daprClient.PublishEventAsync(PUBSUB_NAME, nameof(PaymentConfirmed), paymentRes);
                }
                else
                {
                    var res = new PaymentFailed(status.ToString(), invoiceIssued.customer, invoiceIssued.orderId,
                            invoiceIssued.items, invoiceIssued.totalInvoice, invoiceIssued.instanceId);
                    await this.daprClient.PublishEventAsync(PUBSUB_NAME, nameof(PaymentFailed), res);
                    await this.daprClient.PublishEventAsync(PUBSUB_NAME, streamId, new TransactionMark(invoiceIssued.instanceId, TransactionType.CUSTOMER_SESSION, invoiceIssued.customer.CustomerId, MarkStatus.NOT_ACCEPTED, "payment"));
                }
            }
        }
    }

    static readonly string streamId = new StringBuilder(nameof(TransactionMark)).Append('_').Append(TransactionType.CUSTOMER_SESSION.ToString()).ToString();

    public async Task ProcessPoisonPayment(InvoiceIssued paymentRequest)
    {
        await this.daprClient.PublishEventAsync(PUBSUB_NAME, streamId, new TransactionMark(paymentRequest.instanceId, TransactionType.CUSTOMER_SESSION, paymentRequest.customer.CustomerId, MarkStatus.ABORT, "payment"));
    }

    public void Cleanup()
    {
        this.paymentRepository.Cleanup();
    }

}

