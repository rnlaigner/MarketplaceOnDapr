using System.Linq;
using System.Text.Json;
using CartMS.Infra;
using CartMS.Models;
using CartMS.Repositories;
using CartMS.Services;
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

    private readonly ILogger<EventController> logger;
    private readonly ICartService cartService;

    public EventController(DaprClient daprClient,
                            ICartService cartService, 
                            ICartRepository cartRepository,
                            IProductRepository productRepository,
                            ILogger<EventController> logger)
    {
        this.daprClient = daprClient;
        this.cartRepository = cartRepository;
        this.productRepository = productRepository;
        this.logger = logger;
        this.cartService = cartService;
    }

    /*
     * Based on the docs:
     * https://docs.dapr.io/developing-applications/building-blocks/pubsub/pubsub-bulk/
     * TODO separate delete and upserts...
     */
    [HttpPost("BulkProductStreaming")]
    [BulkSubscribe("BulkProductStreaming")]
    [Topic(PUBSUB_NAME, "Products")]
    public async Task<ActionResult<BulkSubscribeAppResponse>> BulkProcessProductStream([FromBody] BulkSubscribeMessage<BulkMessageModel<Product>> bulkMessages )
    {
        List<BulkSubscribeAppResponseEntry> responseEntries; // = new List<BulkSubscribeAppResponseEntry>();
        this.logger.LogInformation($"Received {bulkMessages.Entries.Count()} messages");

        var dict = bulkMessages.Entries.ToDictionary(k => k.EntryId, v => v.Event.Data);

        // https://docs.dapr.io/developing-applications/building-blocks/state-management/howto-get-save-state/
        var ops = bulkMessages.Entries.Select(m => new StateTransactionRequest(m.Event.Data.product_id.ToString(), JsonSerializer.SerializeToUtf8Bytes(m.Event.Data), StateOperationType.Upsert)).ToList();

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
    public ActionResult ProcessProductStream([FromBody] Product product)
    {
        ProductModel? product_ = productRepository.GetProduct(product.seller_id, product.product_id);

        if(product_ is null)
        {
            var now = DateTime.Now;
            product_ = new()
            {
                seller_id = product.seller_id,
                product_id = product.product_id,
                name = product.name,
                sku = product.sku,
                category = product.category,
                description = product.description,
                price = product.price,
                created_at = now,
                updated_at = now,
                status = product.status,
                active = product.active
            };
            this.productRepository.Insert(product_);
        }

        if (product.active)
        {
            this.productRepository.Update(product_);
        } else
        {
            this.productRepository.Delete(product_);
        }


        this.logger.LogInformation("[ProcessProductStream] completed for product ID {0}", product.product_id);
        return Ok();
    }

    /*
     * "In order to tell Dapr that a message was processed successfully, return a 200 OK response. 
     * If Dapr receives any other return status code than 200, or if your app crashes, 
     * Dapr will attempt to redeliver the message following at-least-once semantics."
     * Source: https://docs.dapr.io/developing-applications/building-blocks/pubsub/howto-publish-subscribe/
     * 
     */
    [HttpPost("ProcessPaymentConfirmed")]
    [Topic(PUBSUB_NAME, nameof(PaymentConfirmed))]
    public ActionResult ProcessPaymentConfirmed([FromBody] PaymentConfirmed paymentConfirmed)
    {
        this.logger.LogInformation("[ProcessPaymentConfirmed] received for customer {0}", paymentConfirmed.customer.CustomerId);
        this.cartService.ProcessPaymentConfirmed(paymentConfirmed);
        this.logger.LogInformation("[ProcessPaymentConfirmed] completed for customer {0}.", paymentConfirmed.customer.CustomerId);
        return Ok();
    }

    [HttpPost("ProcessPaymentFailed")]
    [Topic(PUBSUB_NAME, nameof(PaymentFailed))]
    public ActionResult ProcessPaymentFailed([FromBody] PaymentFailed paymentFailed)
    {
        this.logger.LogInformation("[ProcessPaymentConfirmed] received for customer {0}", paymentFailed.customer.CustomerId);
        this.cartService.ProcessPaymentFailed(paymentFailed);
        this.logger.LogInformation("[ProcessPaymentConfirmed] completed for customer {0}.", paymentFailed.customer.CustomerId);
        return Ok();
    }

}