using System;
using Common.Entities;
using ProductMS.Infra;
using ProductMS.Models;

namespace ProductMS.Repositories
{
	public class ProductRepository : IProductRepository
	{

        private readonly ProductDbContext dbContext;

        public ProductRepository(ProductDbContext dbContext)
		{
            this.dbContext = dbContext;
        }

        public void Delete(ProductModel product)
        {
            product.active = false;
            product.updated_at = DateTime.Now;
            this.dbContext.Products.Update(product);
            this.dbContext.SaveChanges();
        }

        public List<ProductModel> GetBySeller(long sellerId)
        {
            return this.dbContext.Products.Where(p => p.seller_id == sellerId).ToList();
        }

        public ProductModel? GetProduct(long sellerId, long productId)
        {
            var product = this.dbContext.Products.Find(sellerId, productId);
            if (product is not null && product.active) return product;
            return null;
        }

        public ProductModel? GetProduct(long productId)
        {
            return this.dbContext.Products.Where(p => p.product_id == productId).FirstOrDefault();
        }

        public void Insert(ProductModel product)
        {
            product.created_at = DateTime.Now;
            this.dbContext.Products.Add(product);
            this.dbContext.SaveChanges();
        }

        public void Update(ProductModel product)
        {
            product.updated_at = DateTime.Now;
            this.dbContext.Products.Update(product);
            this.dbContext.SaveChanges();
        }

    }
}

