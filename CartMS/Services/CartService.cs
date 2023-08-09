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

    public async Task NotifyCheckout(CustomerCheckout customerCheckout)
    {
        using (var txCtx = dbContext.Database.BeginTransaction())
        {
            var cart = this.cartRepository.GetCart(customerCheckout.CustomerId);

            List<ProductStatus> divergencies = this.CheckCartForDivergencies(cart);
            if (divergencies.Count() > 0)
            {
                if (config.CartStreaming)
                {
                    await ProcessPoisonCheckout(customerCheckout, MarkStatus.NOT_ACCEPTED);
                }
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
                Vouchers = i.vouchers is null ? emptyArray : Array.ConvertAll(i.vouchers.Split(','), float.Parse)
            }).ToList();

            this.Seal(cart);
            txCtx.Commit();

            if (config.CartStreaming)
            {
                ReserveStock checkout = new ReserveStock(DateTime.UtcNow, customerCheckout, cartItems, customerCheckout.instanceId);
                await this.daprClient.PublishEventAsync(PUBSUB_NAME, nameof(ReserveStock), checkout);
            }
        }
    }

    public async Task ProcessPoisonCheckout(CustomerCheckout customerCheckout, MarkStatus status)
    {
        if (config.CartStreaming)
        {
            await this.daprClient.PublishEventAsync(PUBSUB_NAME, checkoutStreamId, new TransactionMark(customerCheckout.instanceId, TransactionType.CUSTOMER_SESSION, customerCheckout.CustomerId, status, "cart"));
        }
    }

    private List<ProductStatus> CheckCartForDivergencies(CartModel cart)
    {
        var divergencies = new List<ProductStatus>();

        if (config.CheckPriceUpdateOnCheckout)
        {
            var items = cartRepository.GetItems(cart.customer_id);

            var itemsDict = items.ToDictionary(i => (i.seller_id, i.product_id));

            var ids = items.Select(i => (i.seller_id,i.product_id)).ToList();
            IList<ProductModel> products = productRepository.GetProducts(ids);

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
        }

        return divergencies;
    }

    public async Task ProcessProductUpdate(ProductUpdate productUpdate)
    {
        using (var txCtx = dbContext.Database.BeginTransaction())
        {
            ProductModel? product = this.productRepository.GetProduct(productUpdate.seller_id, productUpdate.product_id);
            if (product is null)
            {
                this.logger.LogWarning("[ProcessProductUpdate] seller {0} product {1} has been deleted prior to processing this product update...", productUpdate.seller_id, productUpdate.product_id);
            }
            else
            {
                if (productUpdate.active)
                {
                    product.price = productUpdate.price;
                    this.productRepository.Update(product);
                }
                else
                {
                    this.productRepository.Delete(product);
                }

                txCtx.Commit();
                    
            }
        }
        // has to send only if not a delete otherwise it will cause duplicate tx mark in the driver (given the stock is responsible for issuing the delete mark)
        if (productUpdate.active && config.CartStreaming)
        {
            await this.daprClient.PublishEventAsync(PUBSUB_NAME, priceUpdateStreamId, new TransactionMark(productUpdate.instanceId, TransactionType.PRICE_UPDATE, productUpdate.seller_id, MarkStatus.SUCCESS, "cart"));
        }

    }

    static readonly string priceUpdateStreamId = new StringBuilder(nameof(TransactionMark)).Append('_').Append(TransactionType.PRICE_UPDATE.ToString()).ToString();

    public async Task ProcessPoisonProductUpdate(ProductUpdate productUpdate)
    {
        if (productUpdate.active)
        {
            await this.daprClient.PublishEventAsync(PUBSUB_NAME, priceUpdateStreamId, new TransactionMark(productUpdate.instanceId, TransactionType.PRICE_UPDATE, productUpdate.seller_id, MarkStatus.ABORT, "cart"));
        }
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
