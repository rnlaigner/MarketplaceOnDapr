using System;
using Common.Entities;
using Common.Events;
using Dapr.Client;
using Microsoft.EntityFrameworkCore;
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
        private readonly IExternalProvider externalProvider;
        private readonly ILogger<PaymentService> logger;

        public PaymentService(PaymentDbContext dbContext, DaprClient daprClient, IExternalProvider externalProvider, ILogger<PaymentService> logger)
		{
            this.dbContext = dbContext;
            this.daprClient = daprClient;
            this.externalProvider = externalProvider;
            this.logger = logger;
		}

        /**
         * The code below may commit and crash before sending the
         * result event.
         * 
         * one way to make sure the result event has been sent is
         * subscribing to it. after receiving the event, can update
         * a tracking table. if a payment request is made and the
         * event has not been sent, just send the event without 
         * processing the payment again (just by checking the 
         * idempotency key in tracking table)
         * 
         * however, this method still has flaws
         * since this method can also fail after reading
         * from the queue (and acknowledging the read...).
         * i.e., without transactional queuing
         * is difficult to ensure guarantees
         * 
         * in this sense, the least effort is employed.
         * the result event is generated again and 
         * downstream microservices must ensure 
         * idempotency by themselves
         * 
         */
        public async Task ProcessPayment(InvoiceIssued paymentRequest)
        {

            // check if this request has been made. if not, send it
            var tracking = dbContext.PaymentTrackings.Where(p => p.instanceId == paymentRequest.instanceId).FirstOrDefault();
            bool res = false;
            if (tracking is not null)
            {
                res = tracking.status.Equals("succeeded"); // tracking.status
                this.logger.LogInformation("[ProcessPayment] already processed: {0}. Sending again result payload...", paymentRequest.instanceId); 
            } else {
                try
                {
                    res = await this.ProcessPayment_(paymentRequest);
                } catch(Exception e)
                {
                    bool db = e is DbUpdateConcurrencyException || e is DbUpdateException;
                    this.logger.LogInformation("[ProcessPayment] {0} failed due to a database exception: {1}", paymentRequest.instanceId, e.Message);

                    // TODO put on dead letter queue
                    // await this.daprClient.PublishEventAsync(PUBSUB_NAME, "poisonMessages", paymentRes);

                    return;
                }
            }

            if (res)
            {
                var paymentRes = new PaymentConfirmed(paymentRequest.customer, paymentRequest.order_id, paymentRequest.total_amount, paymentRequest.items, paymentRequest.instanceId);
                await this.daprClient.PublishEventAsync(PUBSUB_NAME, nameof(PaymentConfirmed), paymentRes);
                this.logger.LogInformation("[ProcessPayment] confirmed: {0}.", paymentRequest.instanceId);
            }
            else
            {
                // https://stackoverflow.com/questions/73732696/dapr-pubsub-messages-only-being-received-by-one-subscriber
                // https://github.com/dapr/dapr/issues/3176
                // it seems the problem only happens in k8s:
                // https://v1-0.docs.dapr.io/operations/components/component-schema/
                // https://docs.dapr.io/reference/components-reference/supported-pubsub/setup-mqtt3/
                var paymentRes = new PaymentFailed("payment_failed", paymentRequest.customer, paymentRequest.order_id, paymentRequest.items, paymentRequest.total_amount, paymentRequest.instanceId);
                await this.daprClient.PublishEventAsync(PUBSUB_NAME, nameof(PaymentFailed), paymentRes);
                this.logger.LogInformation("[ProcessPayment] failed: {0}.", paymentRequest.instanceId);
            }
        }

        public async Task<bool> ProcessPayment_(InvoiceIssued paymentRequest)
        {

            PaymentIntent intent = await externalProvider.Create(new PaymentIntentCreateOptions()
            {
                Amount = paymentRequest.total_amount,
                Customer = paymentRequest.customer.CustomerId,
                IdempotencyKey = paymentRequest.instanceId,
                cardOptions = new()
                {
                    Number = paymentRequest.customer.CardNumber,
                    Cvc = paymentRequest.customer.CardSecurityNumber,
                    ExpMonth = paymentRequest.customer.CardExpiration, // TODO process
                    ExpYear = paymentRequest.customer.CardExpiration
                }
            });

            using (var dbContextTransaction = dbContext.Database.BeginTransaction())
            {

                // save in tracking in the ctx of transaction
                dbContext.PaymentTrackings.Add(new()
                {
                    instanceId = paymentRequest.instanceId,
                    status = intent.Status
                });
                dbContext.SaveChanges();

                if (!intent.Status.Equals("succeeded"))
                {
                    dbContextTransaction.Commit();
                    return false;
                }

                int seq = 1;

                var cc = paymentRequest.customer.PaymentType.Equals(PaymentType.CREDIT_CARD.ToString());
                // create payment tuples
                if (cc || paymentRequest.customer.PaymentType.Equals(PaymentType.DEBIT_CARD.ToString()))
                {
                    var cardPaymentLine = new OrderPaymentModel()
                    {
                        order_id = paymentRequest.order_id,
                        payment_sequential = seq,
                        payment_type = cc ? PaymentType.CREDIT_CARD : PaymentType.DEBIT_CARD,
                        payment_installments = paymentRequest.customer.Installments,
                        payment_value = paymentRequest.total_amount
                    };

                    // create an entity for credit card payment details with FK to order payment
                    OrderPaymentCardModel card = new()
                    {
                        order_id = paymentRequest.order_id,
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
                        order_id = paymentRequest.order_id,
                        payment_sequential = seq,
                        payment_type = PaymentType.BOLETO,
                        payment_installments = 1,
                        payment_value = paymentRequest.total_amount
                    });
                    seq++;
                }

                // then one line for each voucher
                foreach(var item in paymentRequest.items)
                {
                    // discount was applied
                    if(item.total_items > item.total_amount)
                    {
                        paymentLines.Add(new OrderPaymentModel()
                        {
                            order_id = paymentRequest.order_id,
                            payment_sequential = seq,
                            payment_type = PaymentType.VOUCHER,
                            payment_installments = 1,
                            payment_value = item.total_items - item.total_amount
                        });
                        seq++;
                    }
                }

                if (paymentLines.Count() > 0)
                    dbContext.OrderPayments.AddRange(paymentLines);
                dbContext.SaveChanges();

                dbContextTransaction.Commit();
            
            }

            return true;

        }
    }
}

