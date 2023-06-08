using Common.Entities;
using Common.Events;
using StockMS.Models;

namespace StockMS.Services
{
	public interface IStockService
	{
        Task ReserveStockAsync(ReserveStock checkout);

        void ConfirmReservation(PaymentConfirmed payment);

        void CancelReservation(PaymentFailed paymentFailure);

        void ProcessProductUpdate(Product product);

        void ProcessProductUpdates(List<Product> products);

        Task CreateStockItem(StockItem stockItem);

        Task IncreaseStock(IncreaseStock increaseStock);
    }
}

