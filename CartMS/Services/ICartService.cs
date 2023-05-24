using Common.Entities;

namespace CartMS.Services
{
	public interface ICartService
	{

        public Task SealIfNecessary(Cart cart);
        public Task SealIfNecessary(string customerId);

        public Task NotifyCheckout(CustomerCheckout customerCheckout);
        public Task<List<ProductStatus>> CheckCartForDivergencies(Cart cart);


    }
}

