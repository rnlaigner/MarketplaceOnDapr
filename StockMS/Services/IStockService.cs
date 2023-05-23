using Common.Entities;
using Common.Events;

namespace StockMS.Services
{
	public interface IStockService
	{
        public Task ReserveStockAsync(ReserveStock checkout);

        public bool ReserveStock(ReserveStock checkout);

        public void ConfirmReservation(PaymentConfirmed payment);

        public void CancelReservation(PaymentFailed paymentFailure);

        public void ProcessProductUpdate(Product product);

        public void ProcessProductUpdates(List<Product> products);

        void CreateStockItem(StockItem stockItem);
    }
}

