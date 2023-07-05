﻿using CartMS.Infra;
using CartMS.Models;

namespace CartMS.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly CartDbContext dbContext;

        private readonly ILogger<ProductRepository> logger;

        public ProductRepository(CartDbContext dbContext, ILogger<ProductRepository> logger)
        {
            this.dbContext = dbContext;
            this.logger = logger;
        }

        public ProductModel Delete(ProductModel product)
        {
            var track = dbContext.Products.Remove(product);
            dbContext.SaveChanges();
            return track.Entity;
        }

        public ProductModel? GetProduct(long sellerId, long productId)
        {
            return dbContext.Products.Find(sellerId, productId);
        }

        public IList<ProductModel> GetProducts(IList<(long, long)> ids)
        {
            List<ProductModel> products = new List<ProductModel>();
            foreach(var entry in ids)
            {
                var res = dbContext.Products.Find(entry.Item1, entry.Item2);
                if (res is not null)
                    products.Add(res);
            }
            return products;
        }

        public ProductModel Insert(ProductModel product)
        {
            var track = dbContext.Products.Add(product);
            dbContext.SaveChanges();
            return track.Entity;
        }

        public ProductModel Update(ProductModel product)
        {
            var track = dbContext.Products.Update(product);
            dbContext.SaveChanges();
            return track.Entity;
        }
    }
}

