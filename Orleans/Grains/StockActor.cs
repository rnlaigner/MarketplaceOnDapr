using Common.Entities;
using Microsoft.Extensions.Logging;
using Orleans.Core;
using Orleans.Interfaces;
using Orleans.Runtime;
using StackExchange.Redis;

namespace Orleans.Grains
{
    public class StockActor : Grain, IStockActor
    {
        ConfigurationOptions option;

        private readonly IPersistentState<StockItem> item;

        private readonly ILogger<StockActor> _logger;

        private IGrainIdentity _identity;

        public StockActor([PersistentState(
            stateName: "cart",
            storageName: Infra.Constants.storage)] IPersistentState<StockItem> state,
           ILogger<StockActor> _logger)
        {
            this.item = state;
            this._logger = _logger;
        }

        public override async Task OnActivateAsync()
        {
            this._identity = this.GetGrainIdentity();
            // get redis connection string from metadata grain. publish TransactionMark after delete. can dispose itself after
        }

        public Task AddItem(StockItem item)
        {
            throw new NotImplementedException();
        }

        public Task<ItemStatus> AttemptReservation(int quantity)
        {
            throw new NotImplementedException();
        }

        public Task CancelReservation(int quantity)
        {
            throw new NotImplementedException();
        }

        public Task ConfirmReservation(int quantity)
        {
            throw new NotImplementedException();
        }

        public Task DeleteItem()
        {
            throw new NotImplementedException();
        }
    }
}
