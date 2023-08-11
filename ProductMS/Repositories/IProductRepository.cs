using ProductMS.Models;

namespace ProductMS.Repositories
{
	public interface IProductRepository
	{
        void Update(ProductModel product);

        void Insert(ProductModel product);

        void Delete(ProductModel product);

        ProductModel GetProduct(int sellerId, int productId);

        List<ProductModel> GetBySeller(int sellerId);
        
    }
}

