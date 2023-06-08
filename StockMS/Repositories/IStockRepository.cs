using StockMS.Models;

namespace StockMS.Repositories
{
	public interface IStockRepository
	{
        StockItemModel? GetItemForUpdate(long sellerId, long productId);

        StockItemModel? GetItem(long sellerId, long productId);

        StockItemModel? GetItem(long productId);

        IEnumerable<StockItemModel> GetAll();

        IEnumerable<StockItemModel> GetItemsForUpdate(List<(long SellerId, long ProductId)> ids);
    }
}

