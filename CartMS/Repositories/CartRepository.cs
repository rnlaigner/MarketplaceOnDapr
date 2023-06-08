using System;
using Common.Entities;
using Dapr.Client;
using CartMS.Controllers;
using Common.Events;
using System.Net;
using CartMS.Infra;
using CartMS.Models;
using static Grpc.Core.Metadata;

namespace CartMS.Repositories
{
    public class CartRepository : ICartRepository
    {
        private readonly CartDbContext dbContext;

        private readonly ILogger<CartRepository> logger;

        public CartRepository(CartDbContext cartDbContext, ILogger<CartRepository> logger)
        {
            this.dbContext = cartDbContext;
            this.logger = logger;
        }

        public CartModel? GetCart(long customerId)
        {
            return this.dbContext.Carts.Find(customerId);
        }

        public CartModel? Delete(long customerId)
        {
            CartModel? cart = GetCart(customerId);
            if (cart is not null)
            {
                dbContext.Remove(cart);
                dbContext.SaveChanges();
                return cart;
            }
            return null;
        }

        public CartModel Insert(CartModel cart)
        {
            var f = dbContext.Add(cart);
            dbContext.SaveChanges();
            return f.Entity;
        }


        public IList<CartItemModel> GetItems(long customerId)
        {
            return dbContext.CartItems.Where(c => c.customer_id == customerId).ToList();
        }

        public CartItemModel AddItem(CartItemModel item)
        {
            var existing = dbContext.CartItems.Find(item.customer_id, item.seller_id, item.product_id);
            if(existing is null)
            {
                var res = dbContext.CartItems.Add(item);
                dbContext.SaveChanges();
                return res.Entity;
            }
            var entity = dbContext.CartItems.Update(item);
            dbContext.SaveChanges();
            return entity.Entity;
        }

        public CartItemModel UpdateItem(CartItemModel item)
        {
            var entity = dbContext.CartItems.Update(item);
            dbContext.SaveChanges();
            return entity.Entity;
        }

        public CartModel Update(CartModel cart)
        {
            cart.updated_at = DateTime.Now;
            var f = dbContext.Update(cart);
            dbContext.SaveChanges();
            return f.Entity;
        }

        public void DeleteItems(long customerId)
        {
            var items = GetItems(customerId);
            dbContext.RemoveRange(items);
            dbContext.SaveChanges();
        }
    }
}

