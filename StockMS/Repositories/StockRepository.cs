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

        private readonly StockDbContext dbContext;
        private readonly ILogger<StockRepository> logger;

        public StockRepository(StockDbContext stockDbContext, ILogger<StockRepository> logger)
		{
            this.dbContext = stockDbContext;
            this.logger = logger;
		}

        public void CancelReservation(List<CartItem> cartitems)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<StockItemModel> GetAll()
        {
            return this.dbContext.StockItems;
        }

        public void IncreaseStock(long productId, int quantity)
        {
            throw new NotImplementedException();

        }

        // for update result in disk writes: https://www.postgresql.org/docs/9.0/explicit-locking.html#LOCKING-ROWS
        private const string sqlQuery = "SELECT * FROM stock_items s WHERE s.product_id IN ({0}) FOR UPDATE";

        public IEnumerable<StockItemModel> GetItemsForUpdate(List<long> ids)
        {
            // is it a read lock?
            // var stockItems = dbContext.StockItems.Where(c => ids.Contains(c.product_id)).ToDictionary(c => c.product_id, c => c);
            return dbContext.StockItems.FromSqlRaw(String.Format(sqlQuery, string.Join(", ", ids)));
        }
    }
}

