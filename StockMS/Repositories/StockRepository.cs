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

        public IEnumerable<StockItemModel> GetAll()
        {
            return this.dbContext.StockItems;
        }

        public void IncreaseStock(long productId, int quantity)
        {
            throw new NotImplementedException();

        }

        // for update result in disk writes: https://www.postgresql.org/docs/9.0/explicit-locking.html#LOCKING-ROWS
        private const string sqlGetItemsForUpdate = "SELECT * FROM stock_items s WHERE s.product_id IN ({0}) FOR UPDATE";

        public IEnumerable<StockItemModel> GetItemsForUpdate(List<long> ids)
        {
            string input = string.Join(", ", ids);
            logger.LogWarning("Input is {0}", input);
            // TODO create a simple endpoint to test this retrieval of items...
            return dbContext.StockItems.FromSqlRaw(String.Format(sqlGetItemsForUpdate, input));
        }

        private const string sqlGetItemForUpdate = "SELECT * FROM stock_items s WHERE s.product_id = {0} FOR UPDATE";

        public StockItemModel? GetItemForUpdate(long id)
        {
            return dbContext.StockItems.FromSqlRaw(String.Format(sqlGetItemForUpdate, id)).FirstOrDefault();
        }

        public StockItemModel? GetItem(long sellerId, long productId)
        {
            return this.dbContext.StockItems.Find(sellerId, productId);
        }

        public StockItemModel? GetItem(long productId)
        {
            return this.dbContext.StockItems.Where(i=>i.product_id == productId).FirstOrDefault();
        }
    }
}

