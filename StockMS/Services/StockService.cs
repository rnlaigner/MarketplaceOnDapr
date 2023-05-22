using Common.Entities;
using Common.Events;
using Dapr.Client;
using Microsoft.EntityFrameworkCore;
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
        private readonly ILogger<StockService> logger;

        public StockService(StockDbContext dbContext, IStockRepository stockRepository, DaprClient daprClient, ILogger<StockService> logger)
        {
            this.dbContext = dbContext;
            this.stockRepository = stockRepository;
            this.daprClient = daprClient;
            this.logger = logger;
        }

        public void ProcessProductUpdates(List<Product> products)
        {
            var itemsToDelete = products.Where(p => !p.active).Select(p=>p.id).ToList();
            if (itemsToDelete.Count() == 0) return;

            using (var txCtx = dbContext.Database.BeginTransaction())
            {
                var stockItems = stockRepository.GetItemsForUpdate(itemsToDelete);
                if (stockItems.Count() == 0)
                {
                    this.logger.LogWarning("Attempt to delete products has failed in Stock DB");
                    return;
                }

                foreach(var stockItem in stockItems)
                {
                    stockItem.active = false;
                }

                
                this.dbContext.UpdateRange(stockItems);

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
                var stockItem = stockRepository.GetItemForUpdate(product.id);
                if(stockItem is null)
                {
                    this.logger.LogWarning("Attempt to delete product that does not exists in Stock DB {0}", product.id);
                    return;
                }

                stockItem.active = false;
                this.dbContext.Update(stockItem);

                txCtx.Commit();

            }
        }

        public void CancelReservation(PaymentFailed payment)
        {
            List<long> ids = payment.items.Select(c => c.product_id).ToList();
            using (var txCtx = dbContext.Database.BeginTransaction())
            {

                var tracking = this.dbContext.StockTracking.Where(c => c.instanceId == payment.instanceId && c.operation == OperationType.RESERVE).FirstOrDefault();
                if (tracking is null)
                {
                    this.logger.LogWarning("An attempt to cancel an unknown reservation has been made. InstanceId {0}", payment.instanceId);
                    return;
                }

                var stockItems = stockRepository.GetItemsForUpdate(ids).ToDictionary(c => c.product_id, c => c);

                foreach (var item in payment.items)
                {

                    var stockItem = stockItems[item.product_id];

                    stockItem.qty_reserved -= item.quantity;
                    stockItem.updated_at = DateTime.Now;

                }

                this.dbContext.UpdateRange(stockItems);

                this.dbContext.StockTracking.Add(new StockTracking(payment.instanceId, OperationType.CONFIRM));

                this.dbContext.SaveChanges();

                txCtx.Commit();
            }
        }

        public void ConfirmReservation(PaymentConfirmed payment)
        {
            List<long> ids = payment.items.Select(c => c.product_id).ToList();
            using (var txCtx = dbContext.Database.BeginTransaction())
            {

                var tracking = this.dbContext.StockTracking.Where(c => c.instanceId == payment.instanceId && c.operation == OperationType.RESERVE).FirstOrDefault();
                if(tracking is null)
                {
                    this.logger.LogWarning("An attempt to confirm an unknown reservation has been made. InstanceId {0}", payment.instanceId);
                    return;
                }

                var stockItems = stockRepository.GetItemsForUpdate(ids).ToDictionary(c => c.product_id, c => c);

                foreach (var item in payment.items)
                {

                    var stockItem = stockItems[item.product_id];

                    stockItem.qty_available -= item.quantity;
                    stockItem.qty_reserved -= item.quantity;
                    stockItem.updated_at = DateTime.Now;

                }

                this.dbContext.UpdateRange(stockItems);

                this.dbContext.StockTracking.Add(new StockTracking(payment.instanceId, OperationType.CONFIRM));

                this.dbContext.SaveChanges();

                txCtx.Commit();
            }

        }

        public bool ReserveStock(ReserveStock checkout)
        {
            bool commit = true;
            try
            {
                List<long> ids = checkout.items.Select(c => c.ProductId).ToList();
                using (var txCtx = dbContext.Database.BeginTransaction())
                {

                    // check if request has already been made
                    var tracking = dbContext.StockTracking.Find(checkout.instanceId);
                    if (tracking is not null)
                    {
                        return tracking.success;
                    }

                    var stockItems = stockRepository.GetItemsForUpdate(ids).ToDictionary(c => c.product_id, c => c);

                    foreach (var item in checkout.items)
                    {

                        if (!stockItems.ContainsKey(item.ProductId) || !stockItems[item.ProductId].active)
                        {
                            commit = false;
                            break;
                        }

                        var stockItem = stockItems[item.ProductId];

                        // blindly increase or check here too (dbms will also check due to the constraint)?
                        if (stockItem.qty_available >= (stockItem.qty_reserved + item.Quantity))
                        {
                            commit = false;
                            break;
                        }

                        // take a look: https://learn.microsoft.com/en-us/ef/core/performance/efficient-updating?tabs=ef7
                        stockItem.qty_reserved += item.Quantity;
                        stockItem.updated_at = DateTime.Now;
                    }

                    if (commit)
                    {
                        this.logger.LogError("Attempt to commmit instance Id {0}", checkout.instanceId);
                        // not sure what is the best strategy. perhaps update one by one in the loop and then later rolling back
                        this.dbContext.UpdateRange(stockItems);
                        this.dbContext.StockTracking.Add(new StockTracking(checkout.instanceId, OperationType.RESERVE));
                        this.dbContext.SaveChanges();
                    }
                    else
                    {
                        this.logger.LogError("Cannot commmit transaction Id {0}", txCtx.TransactionId);
                        this.dbContext.StockTracking.Add(new StockTracking(checkout.instanceId, OperationType.RESERVE, false));
                        this.dbContext.SaveChanges();
                    }

                    txCtx.Commit();
                }

            }
            catch (Exception e)
            {
                // https://learn.microsoft.com/en-us/ef/core/saving/concurrency?tabs=data-annotations
                bool conflict = e is DbUpdateConcurrencyException;
                bool update = e is DbUpdateException;
                this.logger.LogError("Exception (conflict: {0} | update {1}) caught in [ReserveStock] method: {2}", conflict, update, e.Message);
                commit = false;
            }

            return commit;

        }

        public async Task ReserveStockAsync(ReserveStock checkout)
        {
            // https://stackoverflow.com/questions/31273933/setting-transaction-isolation-level-in-net-entity-framework-for-sql-server
            bool commit = true;
            // bool failedMessageSent = false;
            // IDictionary<long, StockItemModel> stockItems = null;
            try
            {
                List<long> ids = checkout.items.Select(c => c.ProductId).ToList();

                // could also do this:
                /*
                var conn = stockDbContext.Database.GetDbConnection();
                var dbTransaction = conn.BeginTransaction(System.Data.IsolationLevel.Serializable);
                */

                using (var txCtx = dbContext.Database.BeginTransaction())
                {

                    // check if request has already been made
                    var tracking = dbContext.StockTracking.Find(checkout.instanceId);
                    if (tracking is null)
                    {

                        var stockItems = stockRepository.GetItemsForUpdate(ids).ToDictionary(c => c.product_id, c => c);

                        List<ProductStatus> unavailable = new();

                        foreach (var item in checkout.items)
                        {

                            if (!stockItems.ContainsKey(item.ProductId) || !stockItems[item.ProductId].active)
                            {
                                commit = false;
                                unavailable.Add(new ProductStatus(item.ProductId, ItemStatus.DELETED));
                                continue;
                            }

                            var stockItem = stockItems[item.ProductId];

                            // blindly increase or check here too?
                            // dbms will also check due to the constraint
                            if (stockItem.qty_available >= (stockItem.qty_reserved + item.Quantity))
                            {
                                commit = false;
                                unavailable.Add(new ProductStatus(item.ProductId, ItemStatus.OUT_OF_STOCK, stockItem.qty_available));
                                continue;
                            }
                            
                            // take a look: https://learn.microsoft.com/en-us/ef/core/performance/efficient-updating?tabs=ef7
                            stockItem.qty_reserved += item.Quantity;
                            stockItem.updated_at = DateTime.Now;
                            
                        }

                        if (commit)
                        {
                            this.logger.LogError("Attempt to commmit instance Id {0}", checkout.instanceId);
                            // not sure what is the best strategy. perhaps update one by one in the loop and then later rolling back
                            this.dbContext.UpdateRange(stockItems);

                            this.dbContext.StockTracking.Add(new StockTracking(checkout.instanceId, OperationType.RESERVE));

                            this.dbContext.SaveChanges();

                            // send to order
                            StockConfirmed checkoutRequest = new StockConfirmed(checkout.createdAt, checkout.customerCheckout, checkout.items, checkout.instanceId);
                            await this.daprClient.PublishEventAsync(PUBSUB_NAME, nameof(StockConfirmed), checkoutRequest);
                        }
                        else
                        {
                            this.logger.LogError("Cannot commmit transaction Id {0}", txCtx.TransactionId);
                            this.dbContext.StockTracking.Add(new StockTracking(checkout.instanceId, OperationType.RESERVE, false));
                            this.dbContext.SaveChanges();

                            // notify cart and customer
                            ReserveStockFailed reserveFailed = new ReserveStockFailed(checkout.createdAt, checkout.customerCheckout, unavailable, checkout.instanceId);
                            await this.daprClient.PublishEventAsync(PUBSUB_NAME, nameof(ReserveStockFailed), reserveFailed);
                        }

                        txCtx.Commit();

                    } else
                    {
                        // send event again depending on the outcome
                        if (tracking.success)
                        {
                            StockConfirmed checkoutRequest = new StockConfirmed(checkout.createdAt, checkout.customerCheckout, checkout.items, checkout.instanceId);
                            await this.daprClient.PublishEventAsync(PUBSUB_NAME, nameof(StockConfirmed), checkoutRequest);
                        } else
                        {
                            ReserveStockFailed reserveFailed = new ReserveStockFailed(checkout.createdAt, checkout.customerCheckout, Enumerable.Empty<ProductStatus>().ToList(), checkout.instanceId);
                            await this.daprClient.PublishEventAsync(PUBSUB_NAME, nameof(ReserveStockFailed), reserveFailed);
                        }
                    }
                    
                }

            }
            catch (Exception e)
            {
                // https://learn.microsoft.com/en-us/ef/core/saving/concurrency?tabs=data-annotations
                bool conflict = e is DbUpdateConcurrencyException;
                bool update = e is DbUpdateException;
                this.logger.LogError("Exception (conflict: {0} | update {1}) caught in [ReserveStock] method: {2}", conflict, update, e.Message);
            }
        }

    }
}

