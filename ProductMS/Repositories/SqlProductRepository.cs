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
            this.dbContext.Products.Remove(product);
        }

        public ProductModel? GetProduct(long id)
        {
            return this.dbContext.Products.Find(id);
        }

        public void Insert(ProductModel product)
        {
            this.dbContext.Products.Add(product);
        }

        public void Update(ProductModel product)
        {
            this.dbContext.Products.Update(product);
        }

        /*
        public ProductModel UpdatePrice(long productId, decimal newPrice)
        {
            var product = this.dbContext.Products.Find(productId);
            if (product is null) throw new Exception("Product ID "+ productId +" cannot be found in the database.");
            product.price = newPrice;
            return product;
        }
        */

    }
}

