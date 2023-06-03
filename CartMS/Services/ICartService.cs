using Common.Entities;
using Common.Events;

namespace CartMS.Services
{
	public interface ICartService
	{
        // can also be used for test
        public Task Seal(Cart cart, bool cleanItems = true);

        public Task NotifyCheckout(CustomerCheckout customerCheckout, Cart cart);
        public Task<List<ProductStatus>> CheckCartForDivergencies(Cart cart);

        void ProcessPaymentConfirmed(PaymentConfirmed paymentConfirmed);
        void ProcessPaymentFailed(PaymentFailed paymentFailed);
    }
}

