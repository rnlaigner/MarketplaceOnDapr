﻿using Common.Driver;
using System.Text;
using Common.Entities;
using Common.Events;
using Common.Requests;
using Dapr.Client;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using ProductMS.Infra;
using ProductMS.Models;
using ProductMS.Repositories;
using System;

namespace ProductMS.Services
{
	public class ProductService : IProductService
	{
        private const string PUBSUB_NAME = "pubsub";

        private readonly ProductDbContext dbContext;
        private readonly IProductRepository productRepository;
        private readonly DaprClient daprClient;
        private readonly ProductConfig config;
        private readonly ILogger<ProductService> logger;

        public ProductService(ProductDbContext dbContext, IProductRepository productRepository, DaprClient daprClient,
                              IOptions<ProductConfig> config, ILogger<ProductService> logger)
        {
            this.dbContext = dbContext;
            this.productRepository = productRepository;
            this.daprClient = daprClient;
            this.config = config.Value;
            this.logger = logger;
        }

        private readonly string streamId = new StringBuilder(nameof(TransactionMark)).Append('_').Append(TransactionType.PRICE_UPDATE.ToString()).ToString();

        /*
         * Maybe use postgresql UPSERT: https://stackoverflow.com/questions/1109061/
         */
        public void ProcessCreateProduct(Product product)
        {
            ProductModel input = Utils.AsProductModel(product);
            using (var txCtx = this.dbContext.Database.BeginTransaction()) {
                var existing = dbContext.Products.Find(product.seller_id, product.product_id);
                if(existing is null)
                    this.productRepository.Insert(input);
                else
                    this.productRepository.Update(input);
                txCtx.Commit();
            }
        }

        public async Task ProcessPriceUpdate(PriceUpdate priceUpdate)
        {
            using (var txCtx = this.dbContext.Database.BeginTransaction())
            {
                var product = this.productRepository.GetProductForUpdate(priceUpdate.sellerId, priceUpdate.productId);

                // check if versions match
                if (product.version.SequenceEqual(priceUpdate.version))
                {
                    product.price = priceUpdate.price;
                    this.productRepository.Update(product);
                    txCtx.Commit();
                    // logger.LogWarning($"Versions match for price update. Product {product.version} == {priceUpdate.version}");
                
                } else // outdated update, no longer applies...
                {
                    // logger.LogWarning($"Versions not not match for price update. Product {product.version} != {priceUpdate.version}");
                }

                // still must send because some cart items may be old
                if (config.Streaming)
                {
                    PriceUpdated update = new(
                         priceUpdate.sellerId,
                         priceUpdate.productId,
                         priceUpdate.price,
                         priceUpdate.version,
                         priceUpdate.instanceId
                    );
                    await this.daprClient.PublishEventAsync(PUBSUB_NAME, nameof(PriceUpdated), update);
                }
            }
        }

        public async Task ProcessPoisonPriceUpdate(PriceUpdate priceUpdate)
        {
            if (config.Streaming)
            {
                this.logger.LogInformation("Publishing transaction mark {0} to seller {1}", priceUpdate.instanceId, priceUpdate.sellerId);
                await this.daprClient.PublishEventAsync(PUBSUB_NAME, streamId, new TransactionMark(priceUpdate.instanceId, TransactionType.PRICE_UPDATE, priceUpdate.sellerId, MarkStatus.ERROR, "product"));
            }    
        }

        public async Task ProcessProductUpdate(Product product)
        {
            using (var txCtx = this.dbContext.Database.BeginTransaction())
            {
                var oldProduct = this.productRepository.GetProductForUpdate(product.seller_id, product.product_id);
                if (oldProduct is null)
                {
                    throw new ApplicationException("Product not found "+product.seller_id +"-" + product.product_id);
                }
                ProductModel input = Utils.AsProductModel(product);

                // keep the old
                input.created_at = oldProduct.created_at;
                this.productRepository.Update(input);

                txCtx.Commit();
                if (config.Streaming)
                {
                    ProductUpdated productUpdated = new(input.seller_id, input.product_id, input.name, input.sku, input.category, input.description, input.price, input.freight_value, input.status, input.version);
                    logger.LogWarning("Publishing ProductUpdated event");
                    await this.daprClient.PublishEventAsync(PUBSUB_NAME, nameof(ProductUpdated), productUpdated);
                }
            }
        }

        public async Task ProcessPoisonProductUpdate(Product product)
        {
            if (config.Streaming)
            {
                this.logger.LogInformation("Publishing transaction mark {0} to seller {1}", product.version, product.seller_id);
                await this.daprClient.PublishEventAsync(PUBSUB_NAME, streamId, new TransactionMark(product.version, TransactionType.PRICE_UPDATE, product.seller_id, MarkStatus.ERROR, "product"));
            }    
        }

        public void Cleanup()
        {
            this.dbContext.Products.ExecuteDelete();
            this.dbContext.SaveChanges();
        }

        public void Reset()
        {
            this.dbContext.Database.ExecuteSqlRaw("UPDATE products SET active=true");
            this.dbContext.SaveChanges();
        }

    }
}