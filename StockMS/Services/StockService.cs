using System.Text;
using Common.Driver;
using Common.Entities;
using Common.Events;
using Dapr.Client;
using StockMS.Infra;
using StockMS.Models;
using StockMS.Repositories;
using Microsoft.Extensions.Options;

namespace StockMS.Services;

public class StockService : IStockService
{
    private const string PUBSUB_NAME = "pubsub";

    private readonly IStockRepository stockRepository;
    private readonly DaprClient daprClient;
    private readonly StockConfig config;
    private readonly ILogger<StockService> logger;

    public StockService(IStockRepository stockRepository, DaprClient daprClient, IOptions<StockConfig> config, ILogger<StockService> logger)
    {
        this.stockRepository = stockRepository;
        this.daprClient = daprClient;
        this.config = config.Value;
        this.logger = logger;
    }

    public async Task ProcessProductUpdate(ProductUpdated productUpdated)
    {
        using (var txCtx = this.stockRepository.BeginTransaction())
        {
            StockItemModel stockItem = this.stockRepository.FindForUpdate(productUpdated.seller_id, productUpdated.product_id);
            if (stockItem is null)
            {
                throw new ApplicationException($"Stock item not found {productUpdated.seller_id}-{productUpdated.product_id}");
            }

            stockItem.version = productUpdated.version;
            this.stockRepository.Update(stockItem);
            txCtx.Commit();

            if (this.config.Streaming)
            {
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
        using (var txCtx = this.stockRepository.BeginTransaction())
        {
            IEnumerable<StockItemModel> items = this.stockRepository.GetItems(ids);
            if (!items.Any())
            {
                this.logger.LogCritical($"No items in checkout were retrieved from Stock state: \n{checkout}");
                await this.daprClient.PublishEventAsync(PUBSUB_NAME, streamUpdateId, new TransactionMark(checkout.instanceId, TransactionType.CUSTOMER_SESSION, checkout.customerCheckout.CustomerId, MarkStatus.ERROR, "stock"));
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
                this.stockRepository.UpdateRange(stockItemsReserved);
                this.stockRepository.FlushUpdates();
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
                        this.logger.LogWarning($"No items in checkout were reserved: \n{checkout}");
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
        using (var txCtx = this.stockRepository.BeginTransaction())
        {
            var items = stockRepository.GetItems(ids);
            var stockItems = items.ToDictionary(p => (p.seller_id, p.product_id), c => c);

            foreach (var item in payment.items)
            {
                var stockItem = stockItems[(item.seller_id,item.product_id)];
                stockItem.qty_reserved -= item.quantity;
                stockItem.updated_at = now;
                this.stockRepository.Update(stockItem);
            }
            this.stockRepository.FlushUpdates();
            txCtx.Commit();
        }
    }

    public void ConfirmReservation(PaymentConfirmed payment)
    {
        var now = DateTime.UtcNow;
        var ids = payment.items.Select(p => (p.seller_id, p.product_id)).ToList();
        using (var txCtx = this.stockRepository.BeginTransaction())
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
                this.stockRepository.Update(stockItem);
            }
            this.stockRepository.FlushUpdates();
            txCtx.Commit();
        }
    }

    public async Task IncreaseStock(IncreaseStock increaseStock)
    {
        using (var txCtx = this.stockRepository.BeginTransaction())
        {

            var item = this.stockRepository.Find(increaseStock.seller_id, increaseStock.product_id);
            if (item is null)
            {
                this.logger.LogWarning("Attempt to lock item {0},{1} has not succeeded", increaseStock.seller_id, increaseStock.product_id);
                throw new Exception("Attempt to lock item " + increaseStock.product_id + " has not succeeded");
            }

            item.qty_available += increaseStock.quantity;
            this.stockRepository.Update(item);
            txCtx.Commit();

            if (this.config.Streaming)
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
            version = stockItem.version
        };
        using (var txCtx = this.stockRepository.BeginTransaction())
        {
            var existing = this.stockRepository.Find(stockItem.seller_id, stockItem.product_id);
            if(existing is null)
                this.stockRepository.Insert(stockItemModel);
            else
                this.stockRepository.Update(stockItemModel);
            this.stockRepository.FlushUpdates();
            txCtx.Commit();
        }
        return Task.CompletedTask;
    }

    public void Cleanup()
    {
        this.stockRepository.Cleanup();
    }

    public void Reset()
    {
        this.stockRepository.Reset(this.config.DefaultInventory);
    }

}

