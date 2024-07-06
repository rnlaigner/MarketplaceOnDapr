using System.Text;
using Common.Driver;
using Common.Entities;
using Common.Events;
using Dapr.Client;
using StockMS.Infra;
using StockMS.Models;
using StockMS.Repositories;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;

namespace StockMS.Services;

public class StockService : IStockService
{
    private const string PUBSUB_NAME = "pubsub";

    private readonly StockDbContext dbContext;
    private readonly IStockRepository stockRepository;
    private readonly DaprClient daprClient;
    private readonly StockConfig config;
    private readonly ILogger<StockService> logger;

    public StockService(StockDbContext dbContext, IStockRepository stockRepository, DaprClient daprClient,
        IOptions<StockConfig> config, ILogger<StockService> logger)
    {
        this.dbContext = dbContext;
        this.stockRepository = stockRepository;
        this.daprClient = daprClient;
        this.config = config.Value;
        this.logger = logger;
    }

    public async Task ProcessProductUpdate(ProductUpdated productUpdated)
    {
        this.logger.LogWarning("Service: ProductUpdated event="+productUpdated.ToString());
        using (var txCtx = dbContext.Database.BeginTransaction())
        {
            StockItemModel stockItem = this.stockRepository.GetItemForUpdate(productUpdated.seller_id, productUpdated.product_id);
            if (stockItem is null)
            {
                throw new ApplicationException("Stock item not found "+productUpdated.seller_id +"-" + productUpdated.product_id);
            }

            stockItem.version = productUpdated.version;
            this.stockRepository.Update(stockItem);
            txCtx.Commit();

            if (config.Streaming)
            {
                this.logger.LogWarning("Publishing TransactionMark event to stream "+streamUpdateId);
                await this.daprClient.PublishEventAsync(PUBSUB_NAME, streamUpdateId, new TransactionMark(productUpdated.version, TransactionType.UPDATE_PRODUCT, productUpdated.seller_id, MarkStatus.SUCCESS, "stock"));
            }
           
        }
    }

    public async Task ProcessPoisonProductUpdate(ProductUpdated productUpdate)
    {
        await this.daprClient.PublishEventAsync(PUBSUB_NAME, streamUpdateId, new TransactionMark(productUpdate.version, TransactionType.UPDATE_PRODUCT, productUpdate.seller_id, MarkStatus.ABORT, "stock"));
    }

    static readonly string streamUpdateId = new StringBuilder(nameof(TransactionMark)).Append('_').Append(TransactionType.UPDATE_PRODUCT.ToString()).ToString();

    // https://stackoverflow.com/questions/31273933/setting-transaction-isolation-level-in-net-entity-framework-for-sql-server
    public async Task ReserveStockAsync(ReserveStock checkout)
    {   
        var ids = checkout.items.Select(c => (c.SellerId, c.ProductId)).ToList();
        using (var txCtx = dbContext.Database.BeginTransaction())
        {
            IEnumerable<StockItemModel> items = this.stockRepository.GetItems(ids);
            if (!items.Any())
            {
                await this.daprClient.PublishEventAsync(PUBSUB_NAME, streamUpdateId, new TransactionMark(checkout.instanceId, TransactionType.CUSTOMER_SESSION, checkout.customerCheckout.CustomerId, MarkStatus.NOT_ACCEPTED, "stock"));
                return;
            }

            var stockItems = items.ToDictionary(i => (i.seller_id, i.product_id), v => v);

            List<ProductStatus> unavailableItems = new();
            List<CartItem> cartItemsReserved = new();
            List<StockItemModel> stockItemsReserved = new();
            var now = DateTime.UtcNow;
            foreach (var item in checkout.items)
            {
                if (!stockItems.ContainsKey((item.SellerId, item.ProductId)) || stockItems[(item.SellerId, item.ProductId)].version != item.Version)
                {
                    unavailableItems.Add(new ProductStatus(item.ProductId, ItemStatus.UNAVAILABLE));
                    continue;
                }

                var stockItem = stockItems[(item.SellerId, item.ProductId)];
                if (stockItem.qty_available < (stockItem.qty_reserved + item.Quantity))
                {
                    unavailableItems.Add(new ProductStatus(item.ProductId, ItemStatus.OUT_OF_STOCK, stockItem.qty_available));
                    continue;
                }

                // take a look: https://learn.microsoft.com/en-us/ef/core/performance/efficient-updating?tabs=ef7
                stockItem.qty_reserved += item.Quantity;
                stockItem.updated_at = now;
                cartItemsReserved.Add(item);
                stockItemsReserved.Add(stockItem);
            }

            if (cartItemsReserved.Count() > 0)
            {
                this.dbContext.UpdateRange(stockItemsReserved);
                int entriesWritten = this.dbContext.SaveChanges();
                txCtx.Commit();
            }

            if (this.config.Streaming)
            {
                if (cartItemsReserved.Count() > 0)
                {
                    // send to order
                    StockConfirmed stockConfirmed = new StockConfirmed(
                        checkout.timestamp,
                        checkout.customerCheckout,
                        cartItemsReserved,
                        checkout.instanceId);
                    await this.daprClient.PublishEventAsync(PUBSUB_NAME, nameof(StockConfirmed), stockConfirmed);
                }

                if (unavailableItems.Count() > 0)
                {
                    // notify customer
                    if(this.config.RaiseStockFailed){
                        ReserveStockFailed reserveFailed = new ReserveStockFailed(checkout.timestamp, checkout.customerCheckout,
                            unavailableItems, checkout.instanceId);
                        await this.daprClient.PublishEventAsync(PUBSUB_NAME, nameof(ReserveStockFailed), reserveFailed);
                    }

                    // corner case: no items reserved
                    if (cartItemsReserved.Count() == 0)
                    {
                        await this.daprClient.PublishEventAsync(PUBSUB_NAME, streamReserveId, new TransactionMark(checkout.instanceId, TransactionType.CUSTOMER_SESSION, checkout.customerCheckout.CustomerId, MarkStatus.NOT_ACCEPTED, "stock"));
                    }

                }

            }

        }
    }

    static readonly string streamReserveId = new StringBuilder(nameof(TransactionMark)).Append('_').Append(TransactionType.CUSTOMER_SESSION.ToString()).ToString();

    public async Task ProcessPoisonReserveStock(ReserveStock reserveStock)
    {
        await this.daprClient.PublishEventAsync(PUBSUB_NAME, streamReserveId, new TransactionMark(reserveStock.instanceId, TransactionType.CUSTOMER_SESSION, reserveStock.customerCheckout.CustomerId, MarkStatus.ABORT, "stock"));
    }

    public void CancelReservation(PaymentFailed payment)
    {
        var now = DateTime.UtcNow;
        var ids = payment.items.Select(p => (p.seller_id, p.product_id)).ToList();
        using (var txCtx = dbContext.Database.BeginTransaction())
        {
            var items = stockRepository.GetItems(ids);
            var stockItems = items.ToDictionary(p => (p.seller_id, p.product_id), c => c);

            foreach (var item in payment.items)
            {
                var stockItem = stockItems[(item.seller_id,item.product_id)];
                stockItem.qty_reserved -= item.quantity;
                stockItem.updated_at = now;
                this.dbContext.Update(stockItem);
            }
            this.dbContext.SaveChanges();
            txCtx.Commit();
        }
    }

    public void ConfirmReservation(PaymentConfirmed payment)
    {
        var now = DateTime.UtcNow;
        var ids = payment.items.Select(p => (p.seller_id, p.product_id)).ToList();
        using (var txCtx = this.dbContext.Database.BeginTransaction())
        {
            IEnumerable<StockItemModel> items = stockRepository.GetItems(ids);
            var stockItems = items.ToDictionary(p => (p.seller_id, p.product_id), c => c);

            foreach (var item in payment.items)
            {
                var stockItem = stockItems[(item.seller_id, item.product_id)];
                stockItem.qty_available -= item.quantity;
                stockItem.qty_reserved -= item.quantity;
                stockItem.order_count++;
                stockItem.updated_at = now;
                this.dbContext.Update(stockItem);
            }
            this.dbContext.SaveChanges();
            txCtx.Commit();
        }
    }

    public async Task IncreaseStock(IncreaseStock increaseStock)
    {
        using (var txCtx = dbContext.Database.BeginTransaction())
        {

            var item = dbContext.StockItems.Find(increaseStock.seller_id, increaseStock.product_id);
            if (item is null)
            {
                this.logger.LogWarning("Attempt to lock item {0},{1} has not succeeded", increaseStock.seller_id, increaseStock.product_id);
                throw new Exception("Attempt to lock item " + increaseStock.product_id + " has not succeeded");
            }

            item.qty_available += increaseStock.quantity;
            stockRepository.Update(item);
            txCtx.Commit();

            if (config.Streaming)
            {
                await this.daprClient.PublishEventAsync(PUBSUB_NAME, nameof(StockItem), new StockItem()
                {
                    seller_id = item.seller_id,
                    product_id = item.product_id,
                    qty_available = item.qty_available,
                    qty_reserved = item.qty_reserved,
                    order_count = item.order_count,
                    ytd = item.ytd,
                    data = item.data
                });
            }
        }
    }

    public Task CreateStockItem(StockItem stockItem)
    {
        var now = DateTime.UtcNow;
        StockItemModel stockItemModel = new StockItemModel()
        {
            product_id = stockItem.product_id,
            seller_id = stockItem.seller_id,
            qty_available = stockItem.qty_available,
            qty_reserved = stockItem.qty_reserved,
            order_count = stockItem.order_count,
            ytd = stockItem.ytd,
            data = stockItem.data,
            version = stockItem.version,
            active = true,
            created_at = now,
            updated_at = now
        };
        using (var txCtx = dbContext.Database.BeginTransaction())
        {
            var existing = dbContext.StockItems.Find(stockItem.seller_id, stockItem.product_id);
            if(existing is null)
                dbContext.StockItems.Add(stockItemModel);
            else
                dbContext.StockItems.Update(stockItemModel);
            dbContext.SaveChanges();
            txCtx.Commit();
        }
        return Task.CompletedTask;
    }

    public void Cleanup()
    {
        this.dbContext.StockItems.ExecuteDelete();
        this.dbContext.SaveChanges();
    }

    public void Reset()
    {
        this.dbContext.Database.ExecuteSqlRaw(string.Format("UPDATE stock_items SET active=true, qty_reserved=0, qty_available={0}",config.DefaultInventory));
        this.dbContext.SaveChanges();
    }

}

