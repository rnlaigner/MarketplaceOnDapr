using StockMS.Models;

namespace StockMS.Repositories
{
	public interface IStockRepository
	{

        StockItemModel? GetItem(long sellerId, long productId);

        StockItemModel? GetItem(long productId);

        IEnumerable<StockItemModel> GetAll();

        IEnumerable<StockItemModel> GetItems(List<(long SellerId, long ProductId)> ids);
    }
}

