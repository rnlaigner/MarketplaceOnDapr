using CartMS.Infra;
using CartMS.Repositories;
using Common.Entities;
using Common.Events;
using Dapr.Client;
using Microsoft.Extensions.Options;

namespace CartMS.Services
{
	public class CartService : ICartService
	{

        private const string PUBSUB_NAME = "pubsub";

        private readonly DaprClient daprClient;
        private readonly ICartRepository cartRepository;
        private readonly IProductRepository productRepository;

        private readonly CartConfig config;
        private readonly ILogger<CartService> logger;

        public CartService(DaprClient daprClient, ICartRepository cartRepository, IProductRepository productRepository,
                            IOptions<CartConfig> config, ILogger<CartService> logger)
		{
            this.daprClient = daprClient;
            this.cartRepository = cartRepository;
            this.productRepository = productRepository;
            this.config = config.Value;
            this.logger = logger;
        }

        public async Task Seal(Cart cart, bool cleanItems = true)
        {
            if(cleanItems)
                cart.items.Clear();
            cart.status = CartStatus.OPEN;
            await this.cartRepository.Save(cart);   
        }

        public async Task NotifyCheckout(CustomerCheckout customerCheckout, Cart cart)
        {
            List<ProductStatus> divergencies = await CheckCartForDivergencies(cart);
            if (divergencies.Count() > 0)
            {
                cart.divergencies = divergencies;
                await this.cartRepository.Save(cart);
                CustomerCheckoutFailed checkoutFailed = new CustomerCheckoutFailed(customerCheckout.CustomerId, divergencies);
                await this.daprClient.PublishEventAsync(PUBSUB_NAME, nameof(CustomerCheckoutFailed), checkoutFailed);
                return;
            }

            cart.status = CartStatus.CHECKOUT_SENT;
            bool res = await this.cartRepository.SafeSave(cart);
            if (!res)
            {
                this.logger.LogWarning("Customer {0} cart has already been submitted to checkout", customerCheckout.CustomerId);
                return;
            }

            // CancellationTokenSource source = new CancellationTokenSource();
            // CancellationToken cancellationToken = source.Token;
            this.logger.LogWarning("Customer {0} cart has been submitted to checkout. Publishing checkout event...", customerCheckout.CustomerId);
            ReserveStock checkout = new ReserveStock(DateTime.Now, customerCheckout, cart.items.Select(c => c.Value).ToList());
            await this.daprClient.PublishEventAsync(PUBSUB_NAME, nameof(ReserveStock), checkout); // , cancellationToken);    
        }

        public async Task<List<ProductStatus>> CheckCartForDivergencies(Cart cart)
        {
            var divergencies = new List<ProductStatus>();
            IList<Product>? products = null;
            if (config.CheckPriceUpdateOnCheckout)
            {
                IReadOnlyList<long> ids = (IReadOnlyList<long>)cart.items.Select(i => i.Value.ProductId).ToList();
                products = await productRepository.GetProducts(ids);

                foreach (var product in products)
                {
                    var currPrice = cart.items[product.product_id].UnitPrice;
                    if (currPrice != product.price)
                    {
                        divergencies.Add(new ProductStatus(product.product_id, ItemStatus.PRICE_DIVERGENCE, product.price, currPrice));
                    }
                }
            }

            if (config.CheckIfProductExistsOnCheckout)
            {
                if (products == null)
                {
                    var ids = (IReadOnlyList<long>)cart.items.Select(i => i.Value.ProductId).ToList();
                    products = await productRepository.GetProducts(ids);
                }

                if (cart.items.Count() > products.Count())
                {
                    var dict = products.ToDictionary(c => c.product_id, c => c);
                    // find missing products
                    foreach (var item in cart.items)
                    {
                        if (!dict.ContainsKey(item.Value.ProductId))
                        {
                            divergencies.Add(new ProductStatus(item.Value.ProductId, ItemStatus.DELETED));
                        }
                    }

                }
            }
            return divergencies;
        }

        public async void ProcessPaymentConfirmed(PaymentConfirmed paymentConfirmed)
        {
            Cart cart = await this.cartRepository.GetCart(paymentConfirmed.customer.CustomerId);
            await this.Seal(cart);
        }

        public async void ProcessPaymentFailed(PaymentFailed paymentFailed)
        {
            Cart cart = await this.cartRepository.GetCart(paymentFailed.customer.CustomerId);
            await this.Seal(cart, false);
        }
    }
}

