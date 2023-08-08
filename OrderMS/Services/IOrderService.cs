using System.Threading.Tasks;
using Common.Events;

namespace OrderMS.Services
{
	public interface IOrderService
	{
        public void ProcessShipmentNotification(ShipmentNotification notification);

        public Task ProcessStockConfirmed(StockConfirmed checkout);

        public void ProcessPaymentConfirmed(PaymentConfirmed paymentConfirmed);

        public void ProcessPaymentFailed(PaymentFailed paymentFailed);

        void Cleanup();

        public Task ProcessPoisonStockConfirmed(StockConfirmed stockConfirmed);
    }
}

