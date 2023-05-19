using System.Linq;
using System.Text.Json;
using CartMS.Infra;
using CartMS.Repositories;
using Common.Entities;
using Common.Events;
using Dapr;
using Dapr.AspNetCore;
using Dapr.Client;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace CartMS.Controllers;

[ApiController]
public class EventController : ControllerBase
{
    private const string PUBSUB_NAME = "pubsub";

    private readonly DaprClient daprClient;
    private readonly ICartRepository cartRepository;
    private readonly IProductRepository productRepository;

    private readonly CartConfig config;
    private readonly ILogger<EventController> logger;
    
    public EventController(DaprClient daprClient, ICartRepository cartRepository, IProductRepository productRepository,
                            IOptions<CartConfig> config, ILogger<EventController> logger)
    {
        this.daprClient = daprClient;
        this.cartRepository = cartRepository;
        this.productRepository = productRepository;
        this.config = config.Value;
        this.logger = logger;
    }

    /*
     * Based on the docs:
     * https://docs.dapr.io/developing-applications/building-blocks/pubsub/pubsub-bulk/
     * TODO separate delete and upserts...
     */
    [BulkSubscribe("BulkProductStreaming")]
    [Topic(PUBSUB_NAME, nameof(Product))]
    public async Task<ActionResult<BulkSubscribeAppResponse>> BulkProcessProductStream([FromBody] BulkSubscribeMessage<BulkMessageModel<Product>> bulkMessages )
    {
        List<BulkSubscribeAppResponseEntry> responseEntries; // = new List<BulkSubscribeAppResponseEntry>();
        this.logger.LogInformation($"Received {bulkMessages.Entries.Count()} messages");

        var dict = bulkMessages.Entries.ToDictionary(k => k.EntryId, v => v.Event.Data);

        // https://docs.dapr.io/developing-applications/building-blocks/state-management/howto-get-save-state/
        var ops = bulkMessages.Entries.Select(m => new StateTransactionRequest(m.Event.Data.id.ToString(), JsonSerializer.SerializeToUtf8Bytes(m.Event.Data), StateOperationType.Upsert)).ToList();

        Task task = this.daprClient.ExecuteStateTransactionAsync(PUBSUB_NAME, ops);
        await task;

        if (task.IsCompletedSuccessfully)
        {
            responseEntries = bulkMessages.Entries.Select(message => new BulkSubscribeAppResponseEntry(message.EntryId, BulkSubscribeAppResponseStatus.SUCCESS)).ToList();
        }
        else
        {
            responseEntries = bulkMessages.Entries.Select(message => new BulkSubscribeAppResponseEntry(message.EntryId, BulkSubscribeAppResponseStatus.RETRY)).ToList();
        }

        return new BulkSubscribeAppResponse(responseEntries);
    }

    [HttpPost("ProductStreaming")]
    [Topic(PUBSUB_NAME, nameof(Product))]
    public async Task<IActionResult> ProcessProductStream([FromBody] Product product)
    {
        // this.logger.LogInformation("[ProcessProductStream] received for instanceId {0}", priceUpdate.instanceId);

        // await this.productService.UpdatePrice(priceUpdate);
        // lookup by sku to differentiate from customer id
        // check if dapr supports this: https://redis.io/commands/select/
        // https://stackoverflow.com/questions/35621324/how-to-create-own-database-in-redis
        // Product product_ = await productRepository.GetProduct(product.sku);

        if (product.active)
        {
            await this.productRepository.Upsert(product);
        } else
        {
            await this.productRepository.Delete(product);
        }

        return Ok();
        // this.logger.LogInformation("[UpdatePrice] completed for instanceId {0}", priceUpdate.instanceId);
    }

    /*
     * "In order to tell Dapr that a message was processed successfully, return a 200 OK response. 
     * If Dapr receives any other return status code than 200, or if your app crashes, 
     * Dapr will attempt to redeliver the message following at-least-once semantics."
     * Source: https://docs.dapr.io/developing-applications/building-blocks/pubsub/howto-publish-subscribe/
     * 
     */
    [HttpPost("NotifyCheckout")]
    [Topic(PUBSUB_NAME, nameof(CustomerCheckout))]
    public async Task<IActionResult> NotifyCheckout([FromBody] CustomerCheckout customerCheckout)
    {

        Cart cart = await this.cartRepository.GetCart(customerCheckout.CustomerId);
        if (cart.status == CartStatus.CHECKOUT_SENT)
        {
            this.logger.LogWarning("Customer {0} cart has already been submitted to checkout", customerCheckout.CustomerId);
            return Ok();
        }

        IList<Product>? products = null;
        if (config.CheckPriceUpdateOnCheckout)
        {
            var ids = (IReadOnlyList<string>) cart.items.Select(i => i.Value.Sku).ToList();
            products = await productRepository.GetProducts( ids );

            var divergencies = new List<ProductStatus>();
            foreach(var product in products)
            {
                var currPrice = cart.items[product.id].UnitPrice;
                if (currPrice != product.price)
                {
                    divergencies.Add(new ProductStatus(product.id, ItemStatus.PRICE_DIVERGENCE, product.price, currPrice));
                }
            }

            if(divergencies.Count() > 0)
            {
                CustomerCheckoutFailed checkoutFailed = new CustomerCheckoutFailed(customerCheckout.CustomerId, divergencies);
                await this.daprClient.PublishEventAsync(PUBSUB_NAME, nameof(CustomerCheckoutFailed), checkoutFailed);
                return Ok();
            }

        }

        if (config.CheckIfProductExistsOnCheckout)
        {
            if(products == null)
            {
                var ids = (IReadOnlyList<string>)cart.items.Select(i => i.Value.Sku).ToList();
                products = await productRepository.GetProducts(ids);
            }

            if(cart.items.Count() > products.Count())
            {
                var dict = products.ToDictionary(c => c.sku, c => c);
                // find missing products
                var divergencies = new List<ProductStatus>();
                foreach (var item in cart.items)
                {
                    
                    if (!dict.ContainsKey(item.Value.Sku))
                    {
                        divergencies.Add(new ProductStatus(item.Value.ProductId, ItemStatus.DELETED));
                    }
                }

                CustomerCheckoutFailed checkoutFailed = new CustomerCheckoutFailed(customerCheckout.CustomerId, divergencies);
                await this.daprClient.PublishEventAsync(PUBSUB_NAME, nameof(CustomerCheckoutFailed), checkoutFailed);
                return Ok();
            }

        }

        cart.status = CartStatus.CHECKOUT_SENT;
        bool res = await this.cartRepository.Checkout(cart);
        if (!res)
        {
            this.logger.LogWarning("Customer {0} cart has already been submitted to checkout", customerCheckout.CustomerId);
            return Ok();
        }

        // CancellationTokenSource source = new CancellationTokenSource();
        // CancellationToken cancellationToken = source.Token;

        ReserveStock checkout = new ReserveStock(DateTime.Now, customerCheckout, cart.items.Select(c=>c.Value).ToList());

        await this.daprClient.PublishEventAsync(PUBSUB_NAME, nameof(ReserveStock), checkout); // , cancellationToken);

        return Ok();
    }


}

