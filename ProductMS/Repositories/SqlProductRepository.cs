using System;
using ProductMS.Infra;
using ProductMS.Models;

namespace ProductMS.Repositories
{
	public class SqlProductRepository : IProductRepository
	{

        private readonly ProductDbContext dbContext;

        public SqlProductRepository(ProductDbContext dbContext)
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

        public ProductModel? GetProduct(long sellerId, long productId)
        {
            var product = this.dbContext.Products.Find(sellerId, productId);
            if (product is not null && product.active) return product;
            return null;
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

