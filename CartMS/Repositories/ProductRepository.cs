using CartMS.Infra;
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
            product.updated_at = DateTime.UtcNow;
            product.active = false;
            var track = dbContext.Products.Update(product);
            dbContext.SaveChanges();
            return track.Entity;
        }

        public ProductModel GetProduct(int sellerId, int productId)
        {
            return dbContext.Products.Where(f=>f.seller_id == sellerId && f.product_id == productId).First();
        }

        public IList<ProductModel> GetProducts(IList<(int, int)> ids)
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
            product.updated_at = DateTime.UtcNow;
            product.created_at = product.updated_at;
            var track = dbContext.Products.Add(product);
            dbContext.SaveChanges();
            return track.Entity;
        }

        public ProductModel Update(ProductModel product)
        {
            product.updated_at = DateTime.UtcNow;
            var track = dbContext.Products.Update(product);
            dbContext.SaveChanges();
            return track.Entity;
        }
    }
}

