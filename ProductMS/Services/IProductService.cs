using Common.Entities;
using Common.Requests;

namespace ProductMS.Services
{
	public interface IProductService
	{
        void ProcessCreateProduct(Product product);

        Task ProcessProductUpdate(Product product);
        Task ProcessPoisonProductUpdate(Product product);

        Task ProcessPriceUpdate(PriceUpdate priceUpdate);
        Task ProcessPoisonPriceUpdate(PriceUpdate product);

        void Cleanup();
        void Reset();
        
    }
}