using System;
using Common.Entities;

namespace CartMS.Repositories
{
	/**
	 * To address the product replication or state sharing
	 * 
	 */
	public interface IProductRepository
	{
        Task<bool> Upsert(Product product);

        Task<bool> Delete(Product product);

        Task<IList<Product>> GetProducts(IReadOnlyList<string> ids);

        Task<Product> GetProduct(string id);
    }
}

