﻿using CartMS.Models;

namespace CartMS.Repositories
{
    public interface ICartRepository
    {
        CartModel? GetCart(int customerId);

        IList<CartItemModel> GetItems(int customerId);

        IList<CartItemModel> GetItemsByProduct(int sellerId, int productId, string version);

        CartItemModel AddItem(CartItemModel item);

        CartModel? Delete(int customerId);

        CartModel Update(CartModel cart);

        CartModel Insert(CartModel cart);

        void DeleteItems(int customerId);

        CartItemModel UpdateItem(CartItemModel item);
    }
}

