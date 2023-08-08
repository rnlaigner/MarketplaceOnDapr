using StockMS.Models;

namespace StockMS.Repositories
{
	public interface IStockRepository
	{
        void Delete(StockItemModel product);

        void Update(StockItemModel product);

        StockItemModel? GetItem(int sellerId, int productId);

        IEnumerable<StockItemModel> GetAll();

        IEnumerable<StockItemModel> GetItems(List<(int SellerId, int ProductId)> ids);
        IEnumerable<StockItemModel> GetBySellerId(int sellerId);

        StockItemModel GetItemForUpdate(int seller_id, int product_id);
    }
}

