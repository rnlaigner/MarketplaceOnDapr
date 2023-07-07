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

        public IEnumerable<StockItemModel> GetItems(List<(long SellerId, long ProductId)> ids)
        {
            IList<StockItemModel> items = new List<StockItemModel>();
            foreach (var id in ids)
            {
                var elem = dbContext.StockItems.Find(id.SellerId, id.ProductId);
                if (elem is not null)
                    items.Add(elem);
            }
            return items;
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

