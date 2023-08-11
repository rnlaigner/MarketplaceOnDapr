using Common.Entities;
using Common.Requests;

namespace ProductMS.Services
{
	public interface IProductService
	{
        Task ProcessCreateProduct(Product product);

        Task ProcessProductUpdate(Product product);

        Task ProcessPriceUpdate(PriceUpdate priceUpdate);

        void Cleanup();
        void Reset();
        
    }
}