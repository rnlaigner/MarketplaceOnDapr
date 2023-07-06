using Common.Entities;
using Common.Requests;

namespace Orleans.Interfaces
{
    public interface IProductActor : IGrainWithIntegerCompoundKey
    {
        public Task AddProduct(Product product);

        public Task<Product> GetProduct();

        public Task DeleteProduct();

        public Task UpdatePrice(UpdatePrice updatePrice);

    }
}
