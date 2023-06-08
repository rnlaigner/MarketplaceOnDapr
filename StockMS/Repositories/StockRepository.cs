using System.Text;
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

        // for update result in disk writes: https://www.postgresql.org/docs/9.0/explicit-locking.html#LOCKING-ROWS
        // https://stackoverflow.com/questions/29479891/where-in-query-with-a-composite-key
        private const string sqlGetItemsForUpdate = "SELECT * FROM stock_items s WHERE (s.seller_id, s.product_id) " +
                                                    "IN ({0}) FOR UPDATE";

        public IEnumerable<StockItemModel> GetItemsForUpdate(List<(long SellerId, long ProductId)> ids)
        {
            var sb = new StringBuilder("(");
            foreach(var key in ids)
            {
                sb.Append("(").Append(key.SellerId).Append(",").Append(key.ProductId).Append(")");
            }
            var input = sb.Append(")").ToString();
            logger.LogWarning("SQL input is {0}", input);
            
            return dbContext.StockItems.FromSqlRaw(String.Format(sqlGetItemsForUpdate, input));
        }

        private const string sqlGetItemForUpdate = "SELECT * FROM stock_items s WHERE s.seller_id = {0} AND s.product_id = {1} FOR UPDATE";

        public StockItemModel? GetItemForUpdate(long sellerId, long productId)
        {
            return dbContext.StockItems.FromSqlRaw(String.Format(sqlGetItemForUpdate, sellerId, productId)).FirstOrDefault();
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

