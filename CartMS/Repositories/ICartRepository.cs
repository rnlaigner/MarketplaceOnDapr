using System;
using CartMS.Models;
using Common.Entities;

namespace CartMS.Repositories
{
    public interface ICartRepository
    {
        CartModel? GetCart(long customerId);

        IList<CartItemModel> GetItems(long customerId);

        CartItemModel AddItem(CartItemModel item);

        CartModel? Delete(long customerId);

        CartModel Update(CartModel cart);

        CartModel Insert(CartModel cart);

        void DeleteItems(long customerId);
    }
}

