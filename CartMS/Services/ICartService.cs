using CartMS.Models;
using Common.Entities;
using Common.Events;
using Common.Integration;

namespace CartMS.Services
{
	public interface ICartService
	{
        // can also be used for test
        public void Seal(CartModel cart, bool cleanItems = true);

        public Task NotifyCheckout(CustomerCheckout customerCheckout, CartModel cart);

        public List<ProductStatus> CheckCartForDivergencies(CartModel cart);
    }
}

