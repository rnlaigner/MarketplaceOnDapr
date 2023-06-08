using Common.Entities;
using Common.Events;
using Dapr.Client;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using StockMS.Infra;
using StockMS.Models;
using StockMS.Repositories;

namespace StockMS.Services
{
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

        public void ProcessProductUpdates(List<Product> products)
        {
            var itemsToDelete = products.Where(p => !p.active).Select(p=> (p.seller_id, p.product_id)).ToList();
            if (itemsToDelete.Count() == 0) return;

            using (var txCtx = dbContext.Database.BeginTransaction())
            {
                var stockItems = stockRepository.GetItemsForUpdate(itemsToDelete);
                if (stockItems.Count() == 0)
                {
                    this.logger.LogWarning("Attempt to delete products ({0}) has failed in Stock DB.",itemsToDelete);
                    return;
                }

                foreach(var stockItem in stockItems)
                {
                    stockItem.active = false;
                }

                this.dbContext.UpdateRange(stockItems);
                this.dbContext.SaveChanges();
                txCtx.Commit();

            }
        }

        /**
         * What if the product has been reserved?
         * the order should proceed and if necessary, be cancelled afterwards by an external process
         */
        public void ProcessProductUpdate(Product product)
        {
            if (product.active) return;
            using (var txCtx = dbContext.Database.BeginTransaction())
            {
                var stockItem = stockRepository.GetItemForUpdate(product.seller_id, product.product_id);
                if(stockItem is null)
                {
                    this.logger.LogWarning("Attempt to delete product that does not exists in Stock DB {0}", product.product_id);
                    return;
                }

                stockItem.active = false;
                this.dbContext.Update(stockItem);
                this.dbContext.SaveChanges();
                txCtx.Commit();

            }
        }

        public void CancelReservation(PaymentFailed payment)
        {
            var ids = payment.items.Select(p => (p.seller_id, p.product_id)).ToList();
            using (var txCtx = dbContext.Database.BeginTransaction())
            {

                var stockItems = stockRepository.GetItemsForUpdate(ids).ToDictionary(p => (p.seller_id, p.product_id), c => c);

                foreach (var item in payment.items)
                {
                    var stockItem = stockItems[(item.seller_id,item.product_id)];
                    stockItem.qty_reserved -= item.quantity;
                    stockItem.updated_at = DateTime.Now;
                }

                this.dbContext.UpdateRange(stockItems);
                this.dbContext.SaveChanges();
                txCtx.Commit();
            }
        }

        public void ConfirmReservation(PaymentConfirmed payment)
        {
            var ids = payment.items.Select(p => (p.seller_id, p.product_id)).ToList();
            using (var txCtx = dbContext.Database.BeginTransaction())
            {

                var stockItems = stockRepository.GetItemsForUpdate(ids).ToDictionary(p => (p.seller_id, p.product_id), c => c);

                foreach (var item in payment.items)
                {
                    var stockItem = stockItems[(item.seller_id, item.product_id)];
                    stockItem.qty_available -= item.quantity;
                    stockItem.qty_reserved -= item.quantity;
                    stockItem.updated_at = DateTime.Now;
                }

                this.dbContext.UpdateRange(stockItems);
                this.dbContext.SaveChanges();
                txCtx.Commit();
            }

        }

        public async Task ReserveStockAsync(ReserveStock checkout)
        {
            // https://stackoverflow.com/questions/31273933/setting-transaction-isolation-level-in-net-entity-framework-for-sql-server
            //try
            //{
                if(checkout.items is null)
                {
                    logger.LogWarning("ReserveStock items is NULL!");
                    return;
                }

                var ids = checkout.items.Select(c => (c.SellerId, c.ProductId)).ToList();

                if(checkout.items.Count() == 0)
                {
                    logger.LogWarning("ReserveStock event has no items to reserve!");
                    return;
                }

                // could also do this:
                /*
                var conn = stockDbContext.Database.GetDbConnection();
                var dbTransaction = conn.BeginTransaction(System.Data.IsolationLevel.Serializable);
                */

                using (var txCtx = dbContext.Database.BeginTransaction())
                {

                    var stockItems = stockRepository.GetItemsForUpdate(ids).ToDictionary(c => c.product_id, c => c);

                    if(stockItems.Count() == 0)
                    {
                        logger.LogWarning("ReserveStock has locked no items!");
                        return;
                    }

                    List<ProductStatus> unavailable = new();
                    List<CartItem> itemsReserved = new();

                    foreach (var item in checkout.items)
                    {

                        if (!stockItems.ContainsKey(item.ProductId) || !stockItems[item.ProductId].active)
                        {
                            unavailable.Add(new ProductStatus(item.ProductId, ItemStatus.DELETED));
                            continue;
                        }

                        var stockItem = stockItems[item.ProductId];

                        // blindly increase or check here too?
                        // dbms will also check due to the constraint
                        if (stockItem.qty_available >= (stockItem.qty_reserved + item.Quantity))
                        {
                            unavailable.Add(new ProductStatus(item.ProductId, ItemStatus.OUT_OF_STOCK, stockItem.qty_available));
                            continue;
                        }
                            
                        // take a look: https://learn.microsoft.com/en-us/ef/core/performance/efficient-updating?tabs=ef7
                        stockItem.qty_reserved += item.Quantity;
                        stockItem.updated_at = DateTime.Now;
                        itemsReserved.Add(item);
                    }

                    if(itemsReserved.Count() > 0)
                    {
                        this.dbContext.UpdateRange(stockItems);
                        this.dbContext.SaveChanges();
                        // send to order
                        StockConfirmed checkoutRequest = new StockConfirmed(checkout.timestamp, checkout.customerCheckout,
                            itemsReserved,
                            checkout.instanceId);
                        await this.daprClient.PublishEventAsync(PUBSUB_NAME, nameof(StockConfirmed), checkoutRequest);
                    }

                    if(unavailable.Count() > 0)
                    {
                        // notify cart and customer
                        ReserveStockFailed reserveFailed = new ReserveStockFailed(checkout.timestamp, checkout.customerCheckout,
                            unavailable, checkout.instanceId);
                        await this.daprClient.PublishEventAsync(PUBSUB_NAME, nameof(ReserveStockFailed), reserveFailed);
                    }

                    txCtx.Commit();
                    
                }

            //}
            //catch (Exception e)
            //{
            //    // https://learn.microsoft.com/en-us/ef/core/saving/concurrency?tabs=data-annotations
            //    bool conflict = e is DbUpdateConcurrencyException;
            //    bool update = e is DbUpdateException;
            //    this.logger.LogError("Exception (conflict: {0} | update {1}) caught in [ReserveStock] method: {2}", conflict, update, e.Message);
            //}
        }

        public async Task CreateStockItem(StockItem stockItem)
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
                // publish stock info
                if(config.StockStreaming)
                    await this.daprClient.PublishEventAsync(PUBSUB_NAME, nameof(StockItem), stockItem);
                txCtx.Commit();
            }
        }

        public async Task IncreaseStock(IncreaseStock increaseStock)
        {
            using (var txCtx = dbContext.Database.BeginTransaction())
            {

                var item = stockRepository.GetItemForUpdate(increaseStock.seller_id, increaseStock.product_id);
                if (item is null)
                {
                    this.logger.LogWarning("Attempt to lock item {0},{1} has not succeeded", increaseStock.seller_id, increaseStock.product_id);
                    throw new Exception("Attempt to lock item " + increaseStock.product_id + " has not succeeded");
                }

                item.qty_available += increaseStock.quantity;

                dbContext.StockItems.Update(item);
                dbContext.SaveChanges();

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
                txCtx.Commit();
            }
        }
    }

}