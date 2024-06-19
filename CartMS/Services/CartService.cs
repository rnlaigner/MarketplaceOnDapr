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
    private readonly IProductRepository productRepository;

    private readonly CartConfig config;
    private readonly ILogger<CartService> logger;

    public CartService(DaprClient daprClient, CartDbContext cartDbContext, ICartRepository cartRepository, IProductRepository productRepository, IOptions<CartConfig> config, ILogger<CartService> logger)
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
        cart.updated_at = DateTime.UtcNow;
        this.cartRepository.Update(cart);   
    }

    private static readonly float[] emptyArray = Array.Empty<float>();

    static readonly string checkoutStreamId = new StringBuilder(nameof(TransactionMark)).Append('_').Append(TransactionType.CUSTOMER_SESSION.ToString()).ToString();

    public async Task<bool> NotifyCheckout(CustomerCheckout customerCheckout)
    {
        using (var txCtx = dbContext.Database.BeginTransaction())
        {
            IList<CartItemModel> items = GetItemsWithoutDivergencies(customerCheckout.CustomerId);
            if (items.Count > 0)
            {
                var cart = this.cartRepository.GetCart(customerCheckout.CustomerId);
                if (cart is null) throw new ApplicationException($"Cart {customerCheckout.CustomerId} not found");
                cart.status = CartStatus.CHECKOUT_SENT;
                this.cartRepository.Update(cart);
                var cartItems = items.Select(i => new CartItem()
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
                txCtx.Commit();
                if (config.CartStreaming)
                {
                    ReserveStock checkout = new ReserveStock(DateTime.UtcNow, customerCheckout, cartItems, customerCheckout.instanceId);
                    await this.daprClient.PublishEventAsync(PUBSUB_NAME, nameof(ReserveStock), checkout);
                }
                return true;
            }

            await ProcessPoisonCheckout(customerCheckout, MarkStatus.NOT_ACCEPTED);
            return false;

        }
    }

    public async Task ProcessPoisonCheckout(CustomerCheckout customerCheckout, MarkStatus status)
    {
        if (config.CartStreaming)
        {
            await this.daprClient.PublishEventAsync(PUBSUB_NAME, checkoutStreamId, new TransactionMark(customerCheckout.instanceId, TransactionType.CUSTOMER_SESSION, customerCheckout.CustomerId, status, "cart"));
        }
    }

    private List<CartItemModel> GetItemsWithoutDivergencies(int customerId)
    {
        var items = cartRepository.GetItems(customerId);
        var itemsDict = items.ToDictionary(i => (i.seller_id, i.product_id));

        var ids = items.Select(i => (i.seller_id, i.product_id)).ToList();
        IList<ProductModel> products = productRepository.GetProducts(ids);

        foreach (var product in products)
        {
            var item = itemsDict[(product.seller_id, product.product_id)];
            var currPrice = item.unit_price;
            if (item.version == product.version && currPrice != product.price)
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
            ProductModel product = this.productRepository.GetProduct(priceUpdate.seller_id, priceUpdate.product_id);
            product.version = priceUpdate.version;
            product.price = priceUpdate.price;
            this.productRepository.Update(product);
            txCtx.Commit();
        }
        if (config.CartStreaming)
        {
            await this.daprClient.PublishEventAsync(PUBSUB_NAME, priceUpdateStreamId, new TransactionMark(priceUpdate.instanceId, TransactionType.PRICE_UPDATE, priceUpdate.seller_id, MarkStatus.SUCCESS, "cart"));
        }

    }

    static readonly string priceUpdateStreamId = new StringBuilder(nameof(TransactionMark)).Append('_').Append(TransactionType.PRICE_UPDATE.ToString()).ToString();

    public async Task ProcessPoisonPriceUpdate(PriceUpdated productUpdate)
    {
         await this.daprClient.PublishEventAsync(PUBSUB_NAME, priceUpdateStreamId, new TransactionMark(productUpdate.instanceId, TransactionType.PRICE_UPDATE, productUpdate.seller_id, MarkStatus.ABORT, "cart"));
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
