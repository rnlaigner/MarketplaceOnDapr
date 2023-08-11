using System;
using CartMS.Models;
using Common.Entities;

namespace CartMS.Repositories
{
	/**
	 * To address the product replication or state sharing
	 * 
	 */
	public interface IProductRepository
	{
		ProductModel Insert(ProductModel product);

		ProductModel Update(ProductModel product);

        ProductModel Delete(ProductModel product);

		IList<ProductModel> GetProducts(IList<(int, int)> ids);

        ProductModel GetProduct(int sellerId, int productId);
    }
}

