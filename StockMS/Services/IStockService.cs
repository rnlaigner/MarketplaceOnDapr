using Common.Entities;
using Common.Events;
using StockMS.Models;

namespace StockMS.Services
{
	public interface IStockService
	{
        public Task ReserveStockAsync(ReserveStock checkout);

        public void ConfirmReservation(PaymentConfirmed payment);

        public void CancelReservation(PaymentFailed paymentFailure);

        public void ProcessProductUpdate(Product product);

        public void ProcessProductUpdates(List<Product> products);

        public Task CreateStockItem(StockItem stockItem);
    }
}

