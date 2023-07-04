using Common.Entities;
using Orleans.Interfaces;
using StackExchange.Redis;

namespace Orleans.Grains
{
    public class StockActor : Grain, IStockActor
    {
        ConfigurationOptions option;
        public StockActor() { }

        public Task AddItem(StockItem item)
        {
            throw new NotImplementedException();
        }

        public Task<ItemStatus> AttemptReservation(long productId, int quantity)
        {
            throw new NotImplementedException();
        }

        public Task CancelReservation(long productId, int quantity)
        {
            throw new NotImplementedException();
        }

        public Task ConfirmReservation(long productId, int quantity)
        {
            throw new NotImplementedException();
        }

        public Task DeleteItem(long productId)
        {
            throw new NotImplementedException();
        }
    }
}
