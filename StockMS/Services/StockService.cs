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

        /**
         * What if the product has been reserved?
         * the order should proceed and if necessary, be cancelled afterwards by an external process
         */
        public void ProcessProductUpdate(ProductUpdate product)
        {
            // discard price updates
            if (product.active) return;
            using (var txCtx = dbContext.Database.BeginTransaction())
            {
                var stockItem = stockRepository.GetItem(product.seller_id, product.product_id);
                if(stockItem is null)
                {
                    this.logger.LogWarning("Attempt to delete product that does not exists in Stock DB {0}", product.product_id);
                    return;
                }

                stockItem.active = false;
                this.dbContext.Update(stockItem);
                this.dbContext.SaveChanges();
                txCtx.Commit();
                if (config.StockStreaming)
                {
                    this.logger.LogInformation("Publishing transaction mark {0} to seller {1}", product.instanceId, product.product_id);
                    string streamId = new StringBuilder(nameof(TransactionMark)).Append('_').Append(TransactionType.DELETE_PRODUCT.ToString()).ToString();
                    this.daprClient.PublishEventAsync(PUBSUB_NAME, streamId, new TransactionMark(product.instanceId, TransactionType.DELETE_PRODUCT, product.seller_id));
                }
            }
           
        }

        public void CancelReservation(PaymentFailed payment)
        {
            var ids = payment.items.Select(p => (p.seller_id, p.product_id)).ToList();
            using (var txCtx = dbContext.Database.BeginTransaction())
            {

                var items = stockRepository.GetItems(ids);
                var stockItems = items.ToDictionary(p => (p.seller_id, p.product_id), c => c);

                foreach (var item in payment.items)
                {
                    var stockItem = stockItems[(item.seller_id,item.product_id)];
                    stockItem.qty_reserved -= item.quantity;
                    stockItem.updated_at = DateTime.Now;
                }

                this.dbContext.UpdateRange(items);
                this.dbContext.SaveChanges();
                txCtx.Commit();
            }
        }

        public void ConfirmReservation(PaymentConfirmed payment)
        {
            var ids = payment.items.Select(p => (p.seller_id, p.product_id)).ToList();
            using (var txCtx = dbContext.Database.BeginTransaction())
            {
                var items = stockRepository.GetItems(ids);
                var stockItems = items.ToDictionary(p => (p.seller_id, p.product_id), c => c);

                foreach (var item in payment.items)
                {
                    var stockItem = stockItems[(item.seller_id, item.product_id)];
                    stockItem.qty_available -= item.quantity;
                    stockItem.qty_reserved -= item.quantity;
                    stockItem.updated_at = DateTime.Now;
                }

                this.dbContext.UpdateRange(items);
                this.dbContext.SaveChanges();
                txCtx.Commit();
            }

        }

        public async Task ReserveStockAsync(ReserveStock checkout)
        {
            // https://stackoverflow.com/questions/31273933/setting-transaction-isolation-level-in-net-entity-framework-for-sql-server
          
            if(checkout.items is null)
            {
                logger.LogWarning("[ReserveStockAsync] ReserveStock event items is NULL!");
                return;
            }

            var ids = checkout.items.Select(c => (c.SellerId, c.ProductId)).ToList();

            if(checkout.items.Count() == 0)
            {
                logger.LogWarning("[ReserveStockAsync] ReserveStock event has no items to reserve!");
                return;
            }

            // could also do this:
            /*
            var conn = stockDbContext.Database.GetDbConnection();
            var dbTransaction = conn.BeginTransaction(System.Data.IsolationLevel.Serializable);
            */

            using (var txCtx = dbContext.Database.BeginTransaction())
            {
                IEnumerable<StockItemModel> items = stockRepository.GetItems(ids);

                var stockItems = items.ToDictionary(i => (i.seller_id, i.product_id), v=>v);

                if (stockItems.Count() == 0)
                {
                    logger.LogWarning("[ReserveStockAsync] ReserveStock has locked no items!");
                    return;
                }

                List<ProductStatus> unavailableItems = new();
                List<CartItem> itemsReserved = new();

                foreach (var item in checkout.items)
                {

                    if (!stockItems.ContainsKey((item.SellerId,item.ProductId)) || !stockItems[(item.SellerId, item.ProductId)].active)
                    {
                        unavailableItems.Add(new ProductStatus(item.ProductId, ItemStatus.DELETED));
                        continue;
                    }

                    var stockItem = stockItems[(item.SellerId, item.ProductId)];

                    // blindly increase or check here too?
                    // dbms will also check due to the constraint
                    if (stockItem.qty_available < (stockItem.qty_reserved + item.Quantity))
                    {
                        unavailableItems.Add(new ProductStatus(item.ProductId, ItemStatus.OUT_OF_STOCK, stockItem.qty_available));
                        continue;
                    }
                            
                    // take a look: https://learn.microsoft.com/en-us/ef/core/performance/efficient-updating?tabs=ef7
                    stockItem.qty_reserved += item.Quantity;
                    stockItem.updated_at = DateTime.Now;
                    itemsReserved.Add(item);
                }

                if (itemsReserved.Count() > 0)
                {
                    this.dbContext.UpdateRange(items);
                    int entriesWritten = this.dbContext.SaveChanges();
                    logger.LogInformation("[ReserveStockAsync] Entries written: {0}", entriesWritten);
                }

                txCtx.Commit();

                if (config.StockStreaming)
                {
                    if (itemsReserved.Count() > 0)
                    {
                        // send to order
                        StockConfirmed checkoutRequest = new StockConfirmed(checkout.timestamp, checkout.customerCheckout,
                            itemsReserved,
                            checkout.instanceId);
                        await this.daprClient.PublishEventAsync(PUBSUB_NAME, nameof(StockConfirmed), checkoutRequest);
                    }

                    if (unavailableItems.Count() > 0)
                    {
                        // notify cart and customer
                        ReserveStockFailed reserveFailed = new ReserveStockFailed(checkout.timestamp, checkout.customerCheckout,
                            unavailableItems, checkout.instanceId);
                        await this.daprClient.PublishEventAsync(PUBSUB_NAME, nameof(ReserveStockFailed), reserveFailed);
                    }

                }
                    
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
            // publish stock info
            //if (config.StockStreaming)
            //{
            //    await this.daprClient.PublishEventAsync(PUBSUB_NAME, nameof(StockItem), stockItem);
            //}
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

        public void Cleanup()
        {
            this.dbContext.StockItems.ExecuteDelete();
            this.dbContext.SaveChanges();
        }

        public void Reset()
        {
            this.dbContext.Database.ExecuteSqlRaw("UPDATE stock_items SET active=true, qty_reserved=0, qty_available=10000");
            this.dbContext.SaveChanges();
        }
    }

}