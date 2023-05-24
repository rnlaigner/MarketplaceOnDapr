using System.Net;
using System.Text.Json;
using Common.Entities;
using Common.Events;
using Dapr;
using Dapr.AspNetCore;
using Dapr.Client;
using Microsoft.AspNetCore.Mvc;
using StockMS.Repositories;
using StockMS.Services;

namespace StockMS.Controllers;

[ApiController]
public class StockController : ControllerBase
{
    private const string PUBSUB_NAME = "pubsub";

    private readonly IStockService stockService;

    private readonly IStockRepository stockRepository;

    private readonly ILogger<StockController> logger;

    public StockController(IStockService stockService, IStockRepository stockRepository, ILogger<StockController> logger)
    {
        this.stockService = stockService;
        this.stockRepository = stockRepository;
        this.logger = logger;
    }

    [HttpGet("/")]
    public IEnumerable<StockItem> Get()
    {
        return this.stockRepository.GetAll().Select(x => new StockItem()
        {
            product_id = x.product_id,
            seller_id = x.seller_id,
            qty_available = x.qty_available,
            qty_reserved = x.qty_reserved,
            order_count = x.order_count
        });
    }

    [HttpPost]
    [ProducesResponseType((int)HttpStatusCode.Created)]
    public IActionResult AddStockItem([FromBody] StockItem stockItem, [FromHeader(Name = "instanceId")] string instanceId)
    {
        this.logger.LogInformation("[AddStockItem] received for instanceId {0}", instanceId);
        this.stockService.CreateStockItem(stockItem);
        this.logger.LogInformation("[AddStockItem] completed for instanceId {0}.", instanceId);
        return StatusCode((int)HttpStatusCode.Created);
    }

    [HttpPost("ReserveStock")]
    [Topic(PUBSUB_NAME, nameof(ReserveStock))]
    public async void ReserveStock(ReserveStock checkout)
    {
        this.logger.LogInformation("[ReserveStock] received for instanceId {0}", checkout.instanceId);
        await this.stockService.ReserveStockAsync(checkout);
        this.logger.LogInformation("[ReserveStock] completed for instanceId {0}", checkout.instanceId);
    }

    [HttpPost("ProductStreaming")]
    [Topic(PUBSUB_NAME, nameof(Product))]
    public IActionResult ProcessProductStream([FromBody] Product product)
    {
        this.stockService.ProcessProductUpdate(product);
        return Ok();
        // this.logger.LogInformation("[UpdatePrice] completed for instanceId {0}", priceUpdate.instanceId);
    }

    [BulkSubscribe("BulkProductStreaming")]
    [Topic(PUBSUB_NAME, nameof(Product))]
    public async Task<ActionResult<BulkSubscribeAppResponse>> BulkProcessProductStream([FromBody] BulkSubscribeMessage<BulkMessageModel<Product>> bulkMessages)
    {

        this.stockService.ProcessProductUpdates(bulkMessages.Entries.Select(e=>e.Event.Data).ToList());

        List<BulkSubscribeAppResponseEntry> 
            responseEntries = bulkMessages.Entries.Select(message => new BulkSubscribeAppResponseEntry(message.EntryId, BulkSubscribeAppResponseStatus.SUCCESS)).ToList();
      
        return new BulkSubscribeAppResponse(responseEntries);
    }

}

