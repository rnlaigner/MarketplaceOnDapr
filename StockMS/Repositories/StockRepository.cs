using System.Text;
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

        public void Delete(StockItemModel product)
        {
            product.active = false;
            Update(product);
        }

        public void Update(StockItemModel item)
        {
            item.updated_at = DateTime.UtcNow;
            this.dbContext.StockItems.Update(item);
            this.dbContext.SaveChanges();
        }

        public IEnumerable<StockItemModel> GetAll()
        {
            return this.dbContext.StockItems;
        }

        // https://www.postgresql.org/docs/current/explicit-locking.html#LOCKING-ROWS
        private const string sqlGetItemsForUpdate = "SELECT * FROM stock_items s WHERE (s.seller_id, s.product_id) IN ({0}) order by s.seller_id, s.product_id FOR UPDATE";

        public IEnumerable<StockItemModel> GetItems(List<(long SellerId, long ProductId)> ids)
        {
            var sb = new StringBuilder();
            foreach (var (SellerId, ProductId) in ids)
            {
                sb.Append('(').Append(SellerId).Append(',').Append(ProductId).Append("),");
            }
            var input = sb.Remove(sb.Length - 1,1).ToString();
            logger.LogDebug("SQL input is {0}", input);
            var sql = string.Format(sqlGetItemsForUpdate, input);
            logger.LogDebug("SQL is {0}", sql);
            return dbContext.StockItems.FromSqlRaw(sql);
        }

        public StockItemModel? GetItem(long sellerId, long productId)
        {
            return this.dbContext.StockItems.Find(sellerId, productId);
        }

        public IEnumerable<StockItemModel> GetBySellerId(long sellerId)
        {
            return this.dbContext.StockItems.Where(p => p.seller_id == sellerId);
        }

    }
}