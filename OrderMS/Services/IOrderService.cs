using System;
using System.Threading.Tasks;
using Common.Events;

namespace OrderMS.Services
{
	public interface IOrderService
	{
        public void ProcessShipmentNotification(ShipmentNotification notification);

        public InvoiceIssued ProcessCheckout(StockConfirmed checkout);

        public Task ProcessCheckoutAsync(StockConfirmed checkout);

        public void ProcessPaymentConfirmed(PaymentConfirmed paymentConfirmed);

        public void ProcessPaymentFailed(PaymentFailed paymentFailed);
    }
}

