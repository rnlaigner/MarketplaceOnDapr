using System.Net;
using Common.Entities;
using Common.Events;
using Microsoft.AspNetCore.Mvc;
using StockMS.Models;
using StockMS.Repositories;
using StockMS.Services;

namespace StockMS.Controllers;

[ApiController]
public class StockController : ControllerBase
{

    private readonly IStockService stockService;

    private readonly IStockRepository stockRepository;

    private readonly ILogger<StockController> logger;

    public StockController(IStockService stockService, IStockRepository stockRepository, ILogger<StockController> logger)
    {
        this.stockService = stockService;
        this.stockRepository = stockRepository;
        this.logger = logger;
    }

    [HttpPatch("/")]
    [ProducesResponseType((int)HttpStatusCode.Accepted)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public async Task<ActionResult> IncreaseStock([FromBody] IncreaseStock increaseStock)
    {
        this.logger.LogInformation("[IncreaseStock] received for item id {0}", increaseStock.product_id);
        var item = this.stockRepository.GetItem(increaseStock.seller_id, increaseStock.product_id);
        if (item is null)
        {
            this.logger.LogInformation("[IncreaseStock] completed for item id {0}.", increaseStock.product_id);
            return NotFound();
        }
        await this.stockService.IncreaseStock(increaseStock);
        return Accepted();
    }

    [HttpPost("/")]
    [ProducesResponseType((int)HttpStatusCode.Created)]
    [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
    public async Task<ActionResult> AddStockItem([FromBody] StockItem stockItem)
    {
        this.logger.LogInformation("[AddStockItem] received for item id {0}", stockItem.product_id);
        Task t = this.stockService.CreateStockItem(stockItem);
        await t;
        if (t.IsCompletedSuccessfully)
        {
            this.logger.LogInformation("[AddStockItem] completed for item id {0}.", stockItem.product_id);
            return StatusCode((int)HttpStatusCode.Created);
        }
        return StatusCode((int)HttpStatusCode.InternalServerError);
    }

    [HttpGet("{itemId}")]
    [ProducesResponseType(typeof(StockItem), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public ActionResult<StockItem> GetStockItem(long itemId)
    {
        this.logger.LogInformation("[GetStockItem] received for item id {0}", itemId);
        StockItemModel? item = this.stockRepository.GetItem(itemId);
        this.logger.LogInformation("[GetStockItem] completed for item id {0}.", itemId);
        if (item is not null)
            return Ok(new StockItem()
            {
                seller_id = item.seller_id,
                product_id = item.product_id,
                qty_available = item.qty_available,
                qty_reserved = item.qty_reserved,
                order_count = item.order_count,
                ytd = item.ytd,
                data = item.data
            });
        
        return NotFound();
    }

    [Route("/cleanup")]
    [HttpPatch]
    [ProducesResponseType((int)HttpStatusCode.Accepted)]
    public ActionResult Cleanup()
    {
        this.stockService.Cleanup();
        return Ok();
    }

}