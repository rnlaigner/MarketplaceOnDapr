using System.Security.Cryptography.X509Certificates;
using CartMS.Infra;
using CartMS.Models;
using CartMS.Repositories;
using Common.Entities;
using Common.Events;
using Common.Integration;
using Dapr.Client;
using Microsoft.Extensions.Options;

namespace CartMS.Services
{
	public class CartService : ICartService
	{

        private const string PUBSUB_NAME = "pubsub";

        private readonly DaprClient daprClient;
        private readonly CartDbContext dbContext;

        private readonly ICartRepository cartRepository;
        private readonly IProductRepository productRepository;

        private readonly CartConfig config;
        private readonly ILogger<CartService> logger;

        public CartService(DaprClient daprClient, CartDbContext cartDbContext, ICartRepository cartRepository, IProductRepository productRepository,
                            IOptions<CartConfig> config, ILogger<CartService> logger)
		{
            this.daprClient = daprClient;
            this.dbContext = cartDbContext;
            this.cartRepository = cartRepository;
            this.productRepository = productRepository;
            this.config = config.Value;
            this.logger = logger;
        }

        public void Seal(CartModel cart, bool cleanItems = true)
        {
            cart.status = CartStatus.OPEN;
            if (cleanItems)
            {
                cartRepository.DeleteItems(cart.customer_id);
            }
            cart.updated_at = DateTime.Now;
            this.cartRepository.Update(cart);   
        }

        private static readonly decimal[] emptyArray = Array.Empty<decimal>();

        public async Task NotifyCheckout(CustomerCheckout customerCheckout, CartModel cart)
        {
            using (var txCtx = dbContext.Database.BeginTransaction())
            {
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
                    Vouchers = i.vouchers is null ? emptyArray : Array.ConvertAll(i.vouchers.Split(','), decimal.Parse)
                }).ToList();

                ReserveStock checkout = new ReserveStock(DateTime.Now, customerCheckout, cartItems, customerCheckout.instanceId);
                await this.daprClient.PublishEventAsync(PUBSUB_NAME, nameof(ReserveStock), checkout);
                this.logger.LogInformation("Customer {0} cart has been submitted to checkout.", customerCheckout.CustomerId);
                this.Seal(cart);

                txCtx.Commit();
            }
        }

        public List<ProductStatus> CheckCartForDivergencies(CartModel cart)
        {
            var divergencies = new List<ProductStatus>();

            if (config.CheckPriceUpdateOnCheckout)
            {
                var items = cartRepository.GetItems(cart.customer_id);

                var itemsDict = items.ToDictionary(i => (i.seller_id, i.product_id));

                var ids = items.Select(i => (i.seller_id,i.product_id)).ToList();
                IList<ProductModel> products = productRepository.GetProducts(ids);

                using (var txCtx = dbContext.Database.BeginTransaction())
                {
                    foreach (var product in products)
                    {
                        var item = itemsDict[(product.seller_id, product.product_id)];
                        var currPrice = item.unit_price;
                        if (currPrice != product.price)
                        {
                            item.unit_price = product.price;
                            cartRepository.UpdateItem(item);
                            divergencies.Add(new ProductStatus(product.product_id, ItemStatus.PRICE_DIVERGENCE, product.price, currPrice));
                        }
                    }
                    txCtx.Commit();
                }
            }

            /*
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

    }
}