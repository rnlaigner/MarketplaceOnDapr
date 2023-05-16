using System;
using Common.Entities;
using StockMS.Models;

namespace StockMS.Repositories
{
	public interface IStockRepository
	{
        public IEnumerable<StockItemModel> GetItemsForUpdate(List<long> ids);

        //public void ConfirmReservation(List<CartItem> cartitems);

        public void CancelReservation(List<CartItem> cartitems);

        public void IncreaseStock(long productId, int quantity);

        public IEnumerable<StockItemModel> GetAll();

    }
}

