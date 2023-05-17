using System;
using Common.Entities;
using Common.Events;
using Dapr.Client;
using Microsoft.EntityFrameworkCore;
using StockMS.Infra;
using StockMS.Models;
using StockMS.Repositories;

namespace StockMS.Services
{
	public class StockService
	{

        private const string PUBSUB_NAME = "pubsub";

        private readonly StockDbContext dbContext;
        private readonly IStockRepository stockRepository;
        private readonly DaprClient daprClient;
        private readonly ILogger<StockService> logger;

        public StockService(StockDbContext stockDbContext, IStockRepository stockRepository, DaprClient daprClient, ILogger<StockService> logger)
        {
            this.dbContext = stockDbContext;
            this.stockRepository = stockRepository;
            this.daprClient = daprClient;
            this.logger = logger;
        }

        public async Task ReserveStock(ReserveStock checkout)
        {
            // https://stackoverflow.com/questions/31273933/setting-transaction-isolation-level-in-net-entity-framework-for-sql-server
            bool commit = true;
            bool failedMessageSent = false;
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

                        foreach (var item in checkout.items)
                        {

                            if (!stockItems.ContainsKey(item.ProductId))
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

                            // send to order
                            ProcessCheckout checkoutRequest = new ProcessCheckout(checkout.createdAt, checkout.customerCheckout, checkout.items, checkout.instanceId);
                            await this.daprClient.PublishEventAsync(PUBSUB_NAME, nameof(ProcessCheckout), checkoutRequest);

                        }
                        else
                        {
                            this.logger.LogError("Cannot commmit transaction Id {0}", txCtx.TransactionId);
                            this.dbContext.StockTracking.Add(new StockTracking(checkout.instanceId, OperationType.RESERVE, false));
                            this.dbContext.SaveChanges();

                            // notify cart and customer
                            ReserveStockFailed reserveFailed = new ReserveStockFailed(checkout.createdAt, checkout.customerCheckout, checkout.instanceId);
                            await this.daprClient.PublishEventAsync(PUBSUB_NAME, nameof(ReserveStockFailed), reserveFailed);
                        }

                        txCtx.Commit();

                    } else
                    {
                        // send event again depending on the outcome
                        if (tracking.success)
                        {
                            ProcessCheckout checkoutRequest = new ProcessCheckout(checkout.createdAt, checkout.customerCheckout, checkout.items, checkout.instanceId);
                            await this.daprClient.PublishEventAsync(PUBSUB_NAME, nameof(ProcessCheckout), checkoutRequest);
                        } else
                        {
                            ReserveStockFailed reserveFailed = new ReserveStockFailed(checkout.createdAt, checkout.customerCheckout, checkout.instanceId);
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

