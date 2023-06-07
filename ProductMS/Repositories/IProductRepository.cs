using System;
using ProductMS.Models;

namespace ProductMS.Repositories
{
	public interface IProductRepository
	{
        void Update(ProductModel product);

        void Insert(ProductModel product);

        void Delete(ProductModel product);

        ProductModel? GetProduct(long sellerId, long productId);

        ProductModel? GetProduct(long productId);

        List<ProductModel> GetBySeller(long sellerId);
    }
}

