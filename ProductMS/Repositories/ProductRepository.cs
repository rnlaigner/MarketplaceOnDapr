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
            Update(product);
        }

        public List<ProductModel> GetBySeller(int sellerId)
        {
            return this.dbContext.Products.Where(p => p.seller_id == sellerId).ToList();
        }

        public ProductModel GetProduct(int sellerId, int productId)
        {
            return this.dbContext.Products.Where(f=> f.seller_id == sellerId && f.product_id == productId).First();
        }

        public void Insert(ProductModel product)
        {
            product.created_at = DateTime.UtcNow;
            this.dbContext.Products.Add(product);
            this.dbContext.SaveChanges();
        }

        public void Update(ProductModel product)
        {
            product.updated_at = DateTime.UtcNow;
            this.dbContext.Products.Update(product);
            this.dbContext.SaveChanges();
        }

    }
}

