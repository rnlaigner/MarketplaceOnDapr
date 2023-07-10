using Common.Entities;
using Common.Requests;

namespace ProductMS.Services
{
	public interface IProductService
	{
        Task ProcessNewProduct(Product productToUpdate);
        Task ProcessDelete(DeleteProduct productToDelete);
        Task ProcessUpdate(UpdatePrice update);
        void Cleanup();
        void Reset();
    }
}