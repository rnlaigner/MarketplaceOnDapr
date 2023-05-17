using Common.Entities;
using Common.Events;
using Dapr;
using Dapr.Client;
using Microsoft.AspNetCore.Mvc;
using StockMS.Repositories;
using StockMS.Services;

namespace StockMS.Controllers;

[ApiController]
public class StockController : ControllerBase
{
    private const string PUBSUB_NAME = "pubsub";

    private readonly StockService stockService;

    private readonly DaprClient daprClient;

    private readonly IStockRepository stockRepository;

    private readonly ILogger<StockController> logger;

    public StockController(StockService stockService, IStockRepository stockRepository, DaprClient daprClient,  ILogger<StockController> logger)
    {
        this.stockService = stockService;
        this.daprClient = daprClient;
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


    [HttpPost("ReserveStock")]
    [Topic(PUBSUB_NAME, nameof(Common.Events.ReserveStock))]
    public async void ReserveStock(ReserveStock checkout)
    {
        this.logger.LogInformation("[ReserveStock] received for instanceId {0}", checkout.instanceId);
        await this.stockService.ReserveStock(checkout);
        this.logger.LogInformation("[ReserveStock] completed for instanceId {0}", checkout.instanceId);
    }

}

