using StockMS.Models;

namespace StockMS.Repositories
{
	public interface IStockRepository
	{
        public IEnumerable<StockItemModel> GetItemsForUpdate(List<long> ids);

        public StockItemModel? GetItemForUpdate(long id);

        public StockItemModel? GetItem(long sellerId, long productId);

        public StockItemModel? GetItem(long productId);

        public void IncreaseStock(long productId, int quantity);

        public IEnumerable<StockItemModel> GetAll();

    }
}

