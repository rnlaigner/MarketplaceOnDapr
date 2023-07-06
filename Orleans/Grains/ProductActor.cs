using Common.Entities;
using Common.Requests;
using Orleans.Interfaces;
using Orleans.Runtime;

namespace Orleans.Grains
{
    public class ProductActor : Grain, IProductActor
    {

        private readonly IPersistentState<Product> product;

        public Task AddProduct(Product product)
        {
            throw new NotImplementedException();
        }

        public Task DeleteProduct()
        {
            throw new NotImplementedException();
        }

        public Task<Product> GetProduct()
        {
            throw new NotImplementedException();
        }

        public Task UpdatePrice(UpdatePrice updatePrice)
        {
            throw new NotImplementedException();
        }
    }
}
