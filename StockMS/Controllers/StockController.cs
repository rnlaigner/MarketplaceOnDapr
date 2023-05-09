using Common.Entities;
using Common.Events;
using Dapr;
using Dapr.Client;
using Microsoft.AspNetCore.Mvc;
using StockMS.Repositories;

namespace StockMS.Controllers;

[ApiController]
public class StockController : ControllerBase
{
    private const string PUBSUB_NAME = "pubsub";

    private readonly DaprClient daprClient;

    private readonly IStockRepository stockRepository;

    private readonly ILogger<StockController> _logger;

    public StockController(DaprClient daprClient, IStockRepository stockRepository, ILogger<StockController> logger)
    {
        this.daprClient = daprClient;
        this.stockRepository = stockRepository;
        this._logger = logger;
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
        // .ToList();
    }


    [HttpPost("ReserveInventory")]
    [Topic(PUBSUB_NAME, nameof(Checkout))]
    public async void ReserveInventory(Checkout checkout)
    {
        bool resp = this.stockRepository.Reserve(checkout.items.Select(x => x.Value).ToList());
        if (resp)
        {
            // send to order
            await this.daprClient.PublishEventAsync(PUBSUB_NAME, "ProcessCheckout", checkout);
        } else
        {
            // notify cart and customer
        }
    }

}

