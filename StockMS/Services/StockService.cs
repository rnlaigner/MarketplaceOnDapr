using Common.Driver;
using System.Text;
using Common.Entities;
using Common.Events;
using Dapr.Client;
using Microsoft.Extensions.Options;
using StockMS.Infra;
using StockMS.Models;
using StockMS.Repositories;
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

    /**
     * What if the product has been reserved?
     * the order should proceed and if necessary, be cancelled afterwards by an external process
    */
    public async Task ProcessProductUpdate(ProductUpdate productUpdate)
    {
        // discard price updates, cart will send transaction mark
        if (productUpdate.active) return;

        using (var txCtx = dbContext.Database.BeginTransaction())
        {
            var stockItem = stockRepository.GetItemForUpdate(productUpdate.seller_id, productUpdate.product_id);
            if (stockItem is null)
            {
                this.logger.LogWarning("Attempt to delete product that does not exists in Stock DB {0}|{1}", productUpdate.seller_id, productUpdate.product_id);
                // has to send either case otherwise it can (i) block the driver or (ii) decrease the concurrency level prescribed
                if (config.StockStreaming)
                {
                    await this.daprClient.PublishEventAsync(PUBSUB_NAME, streamDeleteId, new TransactionMark(productUpdate.instanceId, TransactionType.DELETE_PRODUCT, productUpdate.seller_id, MarkStatus.ERROR, "stock"));
                }
            }
            else
            {
                stockRepository.Delete(stockItem);
                txCtx.Commit();
                if (config.StockStreaming)
                {
                    await this.daprClient.PublishEventAsync(PUBSUB_NAME, streamDeleteId, new TransactionMark(productUpdate.instanceId, TransactionType.DELETE_PRODUCT, productUpdate.seller_id, MarkStatus.SUCCESS, "stock"));
                }
            }
        }

    }

    static readonly string streamDeleteId = new StringBuilder(nameof(TransactionMark)).Append('_').Append(TransactionType.DELETE_PRODUCT.ToString()).ToString();

    public async Task ProcessPoisonProductUpdate(ProductUpdate productUpdate)
    {
        await this.daprClient.PublishEventAsync(PUBSUB_NAME, streamDeleteId, new TransactionMark(productUpdate.instanceId, TransactionType.DELETE_PRODUCT, productUpdate.seller_id, MarkStatus.ABORT, "stock"));
    }

    public async Task ReserveStockAsync(ReserveStock checkout)
    {
        // https://stackoverflow.com/questions/31273933/setting-transaction-isolation-level-in-net-entity-framework-for-sql-server

        if (checkout.items is null || checkout.items.Count() == 0)
        {
            await this.daprClient.PublishEventAsync(PUBSUB_NAME, streamReserveId, new TransactionMark(checkout.instanceId, TransactionType.CUSTOMER_SESSION, checkout.customerCheckout.CustomerId, MarkStatus.ERROR, "stock"));
            return;
        }

        var ids = checkout.items.Select(c => (c.SellerId, c.ProductId)).ToList();
        using (var txCtx = dbContext.Database.BeginTransaction())
        {
            IEnumerable<StockItemModel> items = stockRepository.GetItems(ids);

            if (items.Count() == 0)
            {
                await this.daprClient.PublishEventAsync(PUBSUB_NAME, streamDeleteId, new TransactionMark(checkout.instanceId, TransactionType.CUSTOMER_SESSION, checkout.customerCheckout.CustomerId, MarkStatus.ERROR, "stock"));
                return;
            }

            var stockItems = items.ToDictionary(i => (i.seller_id, i.product_id), v => v);

            List<ProductStatus> unavailableItems = new();
            List<CartItem> cartItemsReserved = new();
            List<StockItemModel> stockItemsReserved = new();
            var now = DateTime.UtcNow;
            foreach (var item in checkout.items)
            {

                if (!stockItems.ContainsKey((item.SellerId, item.ProductId)) || !stockItems[(item.SellerId, item.ProductId)].active)
                {
                    unavailableItems.Add(new ProductStatus(item.ProductId, ItemStatus.DELETED));
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

            if (config.StockStreaming)
            {
                if (cartItemsReserved.Count() > 0)
                {
                    // send to order
                    StockConfirmed checkoutRequest = new StockConfirmed(checkout.timestamp, checkout.customerCheckout,
                        cartItemsReserved,
                        checkout.instanceId);
                    await this.daprClient.PublishEventAsync(PUBSUB_NAME, nameof(StockConfirmed), checkoutRequest);
                }

                if (unavailableItems.Count() > 0)
                {
                    // notify customer
                    ReserveStockFailed reserveFailed = new ReserveStockFailed(checkout.timestamp, checkout.customerCheckout,
                        unavailableItems, checkout.instanceId);
                    await this.daprClient.PublishEventAsync(PUBSUB_NAME, nameof(ReserveStockFailed), reserveFailed);

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

    public Task CreateStockItem(StockItem stockItem)
    {
        var stockItemModel = new StockItemModel()
        {
            product_id = stockItem.product_id,
            seller_id = stockItem.seller_id,
            qty_available = stockItem.qty_available,
            qty_reserved = stockItem.qty_reserved,
            order_count = stockItem.order_count,
            ytd = stockItem.ytd,
            data = stockItem.data,

        };
        using (var txCtx = dbContext.Database.BeginTransaction())
        {
            dbContext.StockItems.Add(stockItemModel);
            dbContext.SaveChanges();
            txCtx.Commit();
        }

        return Task.CompletedTask;
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

            if (config.StockStreaming)
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

