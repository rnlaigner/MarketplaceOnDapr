using System.Security.Cryptography.X509Certificates;
using CartMS.Infra;
using CartMS.Models;
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

        public void Seal(CartModel cart, bool cleanItems = true)
        {
            cart.status = CartStatus.OPEN;
            if (cleanItems)
                cartRepository.DeleteItems(cart.customer_id);
            this.cartRepository.Update(cart);   
        }

        public async Task NotifyCheckout(CustomerCheckout customerCheckout, CartModel cart)
        {
            List<ProductStatus> divergencies = await CheckCartForDivergencies(cart);
            if (divergencies.Count() > 0)
            {
                // await this.cartRepository.Save(cart);
                CustomerCheckoutFailed checkoutFailed = new CustomerCheckoutFailed(customerCheckout.CustomerId, divergencies);
                await this.daprClient.PublishEventAsync(PUBSUB_NAME, nameof(CustomerCheckoutFailed), checkoutFailed);
                return;
            }

            cart.status = CartStatus.CHECKOUT_SENT;
            this.cartRepository.Update(cart);

            IList<CartItemModel> items = cartRepository.GetItems(cart.customer_id);

            var cartItems = items.Select(i => new CartItem()
            {
                 SellerId = i.seller_id,
                 ProductId = i.product_id,
                 ProductName = i.product_name,
                 UnitPrice = i.unit_price,
                 FreightValue = i.freight_value,
                 Quantity = i.quantity,
                 Vouchers = i.vouchers
            }).ToList();

            this.logger.LogWarning("Customer {0} cart has been submitted to checkout. Publishing checkout event...", customerCheckout.CustomerId);
            ReserveStock checkout = new ReserveStock(DateTime.Now, customerCheckout, cartItems);
            await this.daprClient.PublishEventAsync(PUBSUB_NAME, nameof(ReserveStock), checkout); // , cancellationToken);    
        }

        public async Task<List<ProductStatus>> CheckCartForDivergencies(CartModel cart)
        {
            var divergencies = new List<ProductStatus>();

            /*
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
            */

            return divergencies;
        }

        public void ProcessPaymentConfirmed(PaymentConfirmed paymentConfirmed)
        {
            CartModel? cart = this.cartRepository.GetCart(paymentConfirmed.customer.CustomerId);
            this.Seal(cart);
        }

        public void ProcessPaymentFailed(PaymentFailed paymentFailed)
        {
            CartModel? cart = this.cartRepository.GetCart(paymentFailed.customer.CustomerId);
            this.Seal(cart, false);
        }
    }
}

