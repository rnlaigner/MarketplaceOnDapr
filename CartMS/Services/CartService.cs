using System.Text;
using CartMS.Infra;
using CartMS.Models;
using CartMS.Repositories;
using Common.Driver;
using Common.Entities;
using Common.Events;
using Common.Requests;
using Dapr.Client;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace CartMS.Services;

public class CartService : ICartService
{

    private const string PUBSUB_NAME = "pubsub";

    private readonly DaprClient daprClient;
    private readonly CartDbContext dbContext;

    private readonly ICartRepository cartRepository;
    private readonly IProductReplicaRepository productReplicaRepository;

    private readonly CartConfig config;
    private readonly ILogger<CartService> logger;

    public CartService(DaprClient daprClient, CartDbContext cartDbContext, ICartRepository cartRepository, IProductReplicaRepository productReplicaRepository, IOptions<CartConfig> config, ILogger<CartService> logger)
	{
        this.daprClient = daprClient;
        this.dbContext = cartDbContext;
        this.cartRepository = cartRepository;
        this.productReplicaRepository = productReplicaRepository;
        this.config = config.Value;
        this.logger = logger;
    }

    public void Seal(CartModel cart, bool cleanItems = true)
    {
        cart.status = CartStatus.OPEN;
        if (cleanItems)
        {
            this.cartRepository.DeleteItems(cart.customer_id);
        }
        cart.updated_at = DateTime.UtcNow;
        this.cartRepository.Update(cart);   
    }

    private static readonly float[] emptyArray = Array.Empty<float>();

    static readonly string checkoutStreamId = new StringBuilder(nameof(TransactionMark)).Append('_').Append(TransactionType.CUSTOMER_SESSION.ToString()).ToString();

    public async Task NotifyCheckout(CustomerCheckout customerCheckout)
    {
        using (var txCtx = dbContext.Database.BeginTransaction())
        {
            List<CartItem> cartItems;
            if(config.ControllerChecks){
                IList<CartItemModel> items = GetItemsWithoutDivergencies(customerCheckout.CustomerId);
                if(items.Count() == 0)
                {
                    throw new ApplicationException($"Cart {customerCheckout.CustomerId} has no items");
                }
                var cart = this.cartRepository.GetCart(customerCheckout.CustomerId);
                if (cart is null) throw new ApplicationException($"Cart {customerCheckout.CustomerId} not found");
                cart.status = CartStatus.CHECKOUT_SENT;
                this.cartRepository.Update(cart);
                cartItems = items.Select(i => new CartItem()
                {
                    SellerId = i.seller_id,
                    ProductId = i.product_id,
                    ProductName = i.product_name is null ? "" : i.product_name,
                    UnitPrice = i.unit_price,
                    FreightValue = i.freight_value,
                    Quantity = i.quantity,
                    Version = i.version,
                    Voucher = i.voucher
                }).ToList();
                this.Seal(cart);
                
            } else {
                IList<CartItemModel> cartItemModels = this.cartRepository.GetItems(customerCheckout.CustomerId);
                this.cartRepository.DeleteItems(customerCheckout.CustomerId);
                cartItems = cartItemModels.Select(i => new CartItem()
                        {
                            SellerId = i.seller_id,
                            ProductId = i.product_id,
                            ProductName = i.product_name is null ? "" : i.product_name,
                            UnitPrice = i.unit_price,
                            FreightValue = i.freight_value,
                            Quantity = i.quantity,
                            Version = i.version,
                            Voucher = i.voucher
                        }).ToList();
            }
            txCtx.Commit();
            if (config.Streaming)
            {
                ReserveStock checkout = new ReserveStock(DateTime.UtcNow, customerCheckout, cartItems, customerCheckout.instanceId);
                await this.daprClient.PublishEventAsync(PUBSUB_NAME, nameof(ReserveStock), checkout);
            }
        }
    }

    public async Task ProcessPoisonCheckout(CustomerCheckout customerCheckout, MarkStatus status)
    {
        if (config.Streaming)
        {
            await this.daprClient.PublishEventAsync(PUBSUB_NAME, checkoutStreamId, new TransactionMark(customerCheckout.instanceId, TransactionType.CUSTOMER_SESSION, customerCheckout.CustomerId, status, "cart"));
        }
    }

    private List<CartItemModel> GetItemsWithoutDivergencies(int customerId)
    {
        var items = cartRepository.GetItems(customerId);
        var itemsDict = items.ToDictionary(i => (i.seller_id, i.product_id));

        var ids = items.Select(i => (i.seller_id, i.product_id)).ToList();
        IList<ProductReplicaModel> products = productReplicaRepository.GetProducts(ids);

        foreach (var product in products)
        {
            var item = itemsDict[(product.seller_id, product.product_id)];
            var currPrice = item.unit_price;
            if (item.version.Equals(product.version) && currPrice != product.price)
            {
                itemsDict.Remove((product.seller_id, product.product_id));
            }
        }
        return itemsDict.Values.ToList();
    }

    public async Task ProcessPriceUpdate(PriceUpdated priceUpdate)
    {
        using (var txCtx = dbContext.Database.BeginTransaction())
        {
            if(this.productReplicaRepository.Exists(priceUpdate.seller_id, priceUpdate.product_id))
            {
                 ProductReplicaModel product = this.productReplicaRepository.GetProductForUpdate(priceUpdate.seller_id, priceUpdate.product_id);
                // if not same version, then it has arrived out of order
                if (product.version.SequenceEqual(priceUpdate.version))
                {
                    product.price = priceUpdate.price;
                    this.productReplicaRepository.Update(product);
                }
                else // outdated update, no longer applies...
                {
                    // logger.LogWarning($"Versions not not match for price update. Product {product.version} != {priceUpdate.version}");
                }
            }

            // update all carts with such version in any case
            var cartItems = this.cartRepository.GetItemsByProduct(priceUpdate.seller_id, priceUpdate.product_id, priceUpdate.version);
            foreach(var item in cartItems)
            {
                item.unit_price = priceUpdate.price;
                item.voucher += priceUpdate.price - item.unit_price;
            }
            txCtx.Commit();
        }
        if (config.Streaming)
        {
            await this.daprClient.PublishEventAsync(PUBSUB_NAME, priceUpdateStreamId, new TransactionMark(priceUpdate.instanceId, TransactionType.PRICE_UPDATE, priceUpdate.seller_id, MarkStatus.SUCCESS, "cart"));
        }

    }

    static readonly string priceUpdateStreamId = new StringBuilder(nameof(TransactionMark)).Append('_').Append(TransactionType.PRICE_UPDATE.ToString()).ToString();

    public async Task ProcessPoisonPriceUpdate(PriceUpdated productUpdate)
    {
         await this.daprClient.PublishEventAsync(PUBSUB_NAME, priceUpdateStreamId, new TransactionMark(productUpdate.instanceId, TransactionType.PRICE_UPDATE, productUpdate.seller_id, MarkStatus.ABORT, "cart"));
    }

    public void ProcessProductUpdated(ProductUpdated productUpdated)
    {
        ProductReplicaModel product_ = new()
        {
            seller_id = productUpdated.seller_id,
            product_id = productUpdated.product_id,
            name = productUpdated.name,
            price = productUpdated.price,
            version = productUpdated.version
        };
        if(this.productReplicaRepository.Exists(productUpdated.seller_id, productUpdated.product_id))
        {
            this.productReplicaRepository.Update(product_);
        } else {
            this.productReplicaRepository.Insert(product_);
        }
    }

    public async Task ProcessPoisonProductUpdated(ProductUpdated productUpdate)
    {
         await this.daprClient.PublishEventAsync(PUBSUB_NAME, priceUpdateStreamId, new TransactionMark(productUpdate.version, TransactionType.UPDATE_PRODUCT, productUpdate.seller_id, MarkStatus.ABORT, "cart"));
    }

    public void Cleanup()
    {
        this.dbContext.Carts.ExecuteDelete();
        this.dbContext.CartItems.ExecuteDelete();
        this.dbContext.Products.ExecuteDelete();
        this.dbContext.SaveChanges();
    }

    public void Reset()
    {
        this.dbContext.Carts.ExecuteDelete();
        this.dbContext.CartItems.ExecuteDelete();
        this.dbContext.Database.ExecuteSqlRaw("UPDATE replica_products SET active=true");
        this.dbContext.SaveChanges();
    }

}
