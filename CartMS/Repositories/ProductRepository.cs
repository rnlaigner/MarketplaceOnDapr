using System;
using CartMS.Infra;
using CartMS.Models;
using Common.Entities;
using Dapr.Client;

namespace CartMS.Repositories
{
    /**
     * Represents the repository of replicated products
     * The format P|{productId} is used to differentiate 
     * from possible clashes with the cart, that uses
     * customerId as key
     */
    public class ProductRepository : IProductRepository
    {
        private readonly CartDbContext cartDbContext;

        private readonly ILogger<ProductRepository> logger;

        public ProductRepository(CartDbContext cartDbContext, ILogger<ProductRepository> logger)
        {
            this.cartDbContext = cartDbContext;
            this.logger = logger;
        }

        public ProductModel Delete(ProductModel product)
        {
            return cartDbContext.Products.Remove(product).Entity;
        }

        public ProductModel? GetProduct(long sellerId, long productId)
        {
            return cartDbContext.Products.Find(sellerId, productId);
        }

        public IList<ProductModel> GetProducts(IList<(long, long)> ids)
        {
            List<ProductModel> products = new List<ProductModel>();
            foreach(var entry in ids)
            {
                var res = cartDbContext.Products.Find(entry.Item1, entry.Item2);
                if (res is not null)
                    products.Add(res);
            }
            return products;
        }

        public ProductModel Insert(ProductModel product)
        {
            return cartDbContext.Products.Add(product).Entity;
        }

        public ProductModel Update(ProductModel product)
        {
            return cartDbContext.Products.Update(product).Entity;
        }
    }
}

