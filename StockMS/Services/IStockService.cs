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

        void ProcessProductUpdate(ProductUpdate product);

        Task CreateStockItem(StockItem stockItem);

        Task IncreaseStock(IncreaseStock increaseStock);

        void Cleanup();
    }
}

