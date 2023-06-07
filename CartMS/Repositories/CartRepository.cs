using System;
using Common.Entities;
using Dapr.Client;
using CartMS.Controllers;
using Common.Events;
using System.Net;
using CartMS.Infra;
using CartMS.Models;

namespace CartMS.Repositories
{
    public class CartRepository : ICartRepository
    {
        private readonly CartDbContext cartDbContext;

        private readonly ILogger<CartRepository> logger;

        public CartRepository(CartDbContext cartDbContext, ILogger<CartRepository> logger)
        {
            this.cartDbContext = cartDbContext;
            this.logger = logger;
        }

        public CartModel? GetCart(long customerId)
        {
            return this.cartDbContext.Carts.Find(customerId);
        }

        public CartModel? Delete(long customerId)
        {
            CartModel? cart = GetCart(customerId);
            if (cart is not null)
            {
                cartDbContext.Remove(cart);
                return cart;
            }
            return null;
        }

        public CartModel Insert(CartModel cart)
        {
            var f = cartDbContext.Add(cart);
            return f.Entity;
        }


        public IList<CartItemModel> GetItems(long customerId)
        {
            return cartDbContext.CartItems.Where(c => c.customer_id == customerId).ToList();
        }

        public CartItemModel AddItem(CartItemModel item)
        {
            var existing = cartDbContext.CartItems.Find(item.customer_id, item.seller_id, item.product_id);
            if(existing is null)
            {
                return cartDbContext.CartItems.Add(item).Entity;
            }
            return cartDbContext.CartItems.Update(item).Entity;
        }

        public CartModel Update(CartModel cart)
        {
            var f = cartDbContext.Update(cart);
            return f.Entity;
        }

        public void DeleteItems(long customerId)
        {
            var items = GetItems(customerId);
            cartDbContext.RemoveRange(items);
        }
    }
}

