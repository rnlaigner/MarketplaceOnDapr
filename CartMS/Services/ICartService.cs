using CartMS.Models;
using Common.Entities;
using Common.Requests;

namespace CartMS.Services
{
	public interface ICartService
	{
        // can also be used for test
        void Seal(CartModel cart, bool cleanItems = true);

        Task NotifyCheckout(CustomerCheckout customerCheckout, CartModel cart);

        List<ProductStatus> CheckCartForDivergencies(CartModel cart);

        void Cleanup();
    }
}

