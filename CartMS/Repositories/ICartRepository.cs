﻿using System;
using Common.Entities;

namespace CartMS.Repositories
{
    public interface ICartRepository
    {
        Task<Cart> GetCart(string customerId);

        Task<bool> AddItem(string customerId, CartItem item);

        Task<bool> Checkout(Cart cart);

        Task Seal(Cart cart);
    }
}

