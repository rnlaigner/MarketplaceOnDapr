using System;
using System.Data;
using System.Transactions;
using Common.Entities;
using Microsoft.EntityFrameworkCore;
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

        public bool Reserve(List<CartItem> cartItems)
        {
            // https://stackoverflow.com/questions/31273933/setting-transaction-isolation-level-in-net-entity-framework-for-sql-server
            bool rollback = false;

            try
            {
                /**
                 * Can't find a way to express this transaction as serializable. Need to set the isolation level in the migration.
                 * 
                 */
                List<long> ids = cartItems.Select(c => c.ProductId).ToList();

                // Npgsql.NpgsqlTransaction

                using (var txCtx = stockDbContext.Database.BeginTransaction())
                {
                    // is it a read lock?
                    var stockItems = stockDbContext.StockItems.Where(c => ids.Contains(c.product_id)).ToDictionary(c => c.product_id, c => c);

                    foreach (var item in cartItems)
                    {

                        if (!stockItems.ContainsKey(item.ProductId))
                        {
                            rollback = true;
                            break;
                        }

                        var stockItem = stockItems[item.ProductId];

                        // blindly increase or check here too (dbms will also check due to the constraint)?
                        if (stockItem.qty_available >= (stockItem.qty_reserved + item.Quantity) )
                        {
                            rollback = true;
                            break;
                        }

                        // take a look: https://learn.microsoft.com/en-us/ef/core/performance/efficient-updating?tabs=ef7
                        stockItem.qty_reserved += item.Quantity;
                        stockItem.updated_at = DateTime.Now;
                    }

                    if (!rollback)
                    {
                        this.logger.LogError("Attempt to commmit transaction Id {0}", txCtx.TransactionId);
                        // not sure what is the best strategy. perhaps update one by one in the loop and then later rolling back
                        this.stockDbContext.UpdateRange(stockItems);
                        this.stockDbContext.SaveChanges();
                        txCtx.Commit();
                        return true;
                    } else
                    {
                        this.logger.LogError("Cannot commmit transaction Id {0}", txCtx.TransactionId);
                        // rollback?
                    }

                }

            } catch(Exception e)
            {
                // https://learn.microsoft.com/en-us/ef/core/saving/concurrency?tabs=data-annotations
                bool conflict = e is DbUpdateConcurrencyException;
                bool update = e is DbUpdateException;
                this.logger.LogError("Exception (conflict: {0} | update {1}) caught in [Reserve] method: {2}", conflict, update, e.Message);
            }
            return false;
        }
    }
}

