using Common.Entities;

namespace Orleans.Interfaces
{
    public interface IStockActor : IGrainWithIntegerKey
    {
        public Task<ItemStatus> AttemptReservation(long productId, int quantity);
        public Task CancelReservation(long productId, int quantity);
        public Task ConfirmReservation(long productId, int quantity);

        public Task DeleteItem(long productId);

        public Task AddItem(StockItem item);

    }
}
