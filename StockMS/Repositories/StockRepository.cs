using System;
using System.Data;
using System.Transactions;
using Common.Entities;
using StockMS.Infra;
using StockMS.Models;

namespace StockMS.Repositories
{
	public class StockRepository : IStockRepository
	{

        private readonly StockDbContext stockDbContext;
        private readonly ILogger<StockRepository> logger;

        public StockRepository(StockDbContext stockDbContext, ILogger<StockRepository> logger)
		{
            this.stockDbContext = stockDbContext;
            this.logger = logger;
		}

        public void CancelReservation(List<CartItem> cartitems)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<StockItemModel> GetAll()
        {
            return this.stockDbContext.StockItems;
        }

        public void IncreaseStock(long productId, int quantity)
        {
            throw new NotImplementedException();
        }

        public bool Reserve(List<CartItem> cartitems)
        {
            // https://stackoverflow.com/questions/31273933/setting-transaction-isolation-level-in-net-entity-framework-for-sql-server
            bool rollback = false;

            try
            {

                using (var txCtx = stockDbContext.Database.BeginTransaction())
                {

                    List<StockItemModel> stockItemsModified = new();
                    // var stockItem = stockDbContext.StockItems.Where(c => c.product_id)
                    foreach (var item in cartitems)
                    {
                        var stockItem = stockDbContext.StockItems.Where(c => c.product_id == item.ProductId).First();

                        if (stockItem is null)
                        {
                            rollback = true;
                            break;
                        }

                        stockItem.qty_reserved += item.Quantity;
                        stockItem.updated_at = DateTime.Now;

                        stockItemsModified.Add(stockItem);
                    }

                    if (!rollback)
                    {
                        // not sure what is the best strategy. perhaps update one by one in the loop and then later rolling back
                        stockDbContext.UpdateRange(stockItemsModified);
                        txCtx.Commit();
                        return true;
                    }
                   
                }

            } catch(Exception e)
            {
                this.logger.LogError("Exception caught in [Reserve] method: {0}", e.Message);
            }
            return false;
        }
    }
}

