using CartMS.Models;
using CartMS.Repositories;
using CartMS.Services;
using Common.Entities;
using Common.Events;
using Dapr;
using Dapr.Client;
using Microsoft.AspNetCore.Mvc;

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

    [HttpPost("ProductStreaming")]
    [Topic(PUBSUB_NAME, nameof(Product))]
    public ActionResult ProcessProductStream([FromBody] Product product)
    {
        var now = DateTime.UtcNow;
        ProductModel product_ = new()
        {
            seller_id = product.seller_id,
            product_id = product.product_id,
            name = product.name,
            price = product.price,
            created_at = now,
            updated_at = now,
            version = product.version
        };
        this.productRepository.Insert(product_);
        return Ok();
    }

    [HttpPost("PriceUpdate")]
    [Topic(PUBSUB_NAME, nameof(PriceUpdated))]
    public async Task<ActionResult> ProcessPriceUpdate([FromBody] PriceUpdated update)
    {
        try
        {
            await this.cartService.ProcessPriceUpdate(update);
        }
        catch (Exception)
        {
            await this.cartService.ProcessPoisonPriceUpdate(update);
        }
        return Ok();
    }

}