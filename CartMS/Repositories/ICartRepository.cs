using System;
using Common.Entities;

namespace CartMS.Repositories
{
    public interface ICartRepository
    {
        Task<Cart> GetCart(long customerId);

        Task<bool> AddItem(long customerId, CartItem item);

        Task<bool> SafeSave(Cart cart);

        Task Save(Cart cart);

        Task<Cart> Delete(long customerId);
    }
}

