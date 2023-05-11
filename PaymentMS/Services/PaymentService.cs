using System;
using Common.Entities;
using Common.Events;
using Microsoft.EntityFrameworkCore;
using PaymentMS.Infra;
using PaymentMS.Integration;
using PaymentMS.Models;
using PaymentMS.Repositories;

namespace PaymentMS.Services
{
	public class PaymentService : IPaymentService
	{

        private readonly PaymentDbContext dbContext;
        private readonly IExternalProvider externalProvider;
        private readonly ILogger<PaymentService> logger;

        public PaymentService(PaymentDbContext paymentDbContext, IExternalProvider externalProvider, ILogger<PaymentService> logger)
		{
            this.dbContext = paymentDbContext;
            this.externalProvider = externalProvider;
            this.logger = logger;
		}

        public bool ProcessPayment(PaymentRequest paymentRequest)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> ProcessPaymentAsync(PaymentRequest paymentRequest)
        {

            // TODO check if this request has been made. if not, send it
            // create idempotency model/table

            PaymentIntent intent = await externalProvider.Create(new PaymentIntentCreateOptions()
            {
                Amount = paymentRequest.total_amount,
                Customer = paymentRequest.customer.CustomerId,
                IdempotencyKey = paymentRequest.instanceId
            });



            using (var dbContextTransaction = dbContext.Database.BeginTransaction())
            {

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
                if (paymentRequest.customer.Vouchers != null)
                {
                    foreach (var voucher in paymentRequest.customer.Vouchers)
                    {
                        paymentLines.Add(new OrderPaymentModel()
                        {
                            order_id = paymentRequest.order_id,
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

                dbContextTransaction.Commit();
            
            }

            return true;

        }
    }
}

