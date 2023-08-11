using Common.Driver;
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

        public async Task ProcessPriceUpdate(PriceUpdate priceUpdate)
        {
            using (var txCtx = this.dbContext.Database.BeginTransaction())
            {
                var product = this.productRepository.GetProduct(priceUpdate.sellerId, priceUpdate.productId);

                if (product is null)
                {
                    // this should not happen
                    logger.LogError("[ProcessProductUpdate] Cannot find seller {0} product {1}.", priceUpdate.sellerId, priceUpdate.productId);

                    if (config.ProductStreaming)
                    {
                        this.logger.LogInformation("Publishing transaction mark {0} to seller {1}", priceUpdate.instanceId, priceUpdate.sellerId);
                        await this.daprClient.PublishEventAsync(PUBSUB_NAME, streamId, new TransactionMark(priceUpdate.instanceId, TransactionType.PRICE_UPDATE, priceUpdate.sellerId, MarkStatus.ERROR, "product"));
                    }
                    return;
                }

                product.updated_at = DateTime.UtcNow;
                product.price = priceUpdate.price;
                this.productRepository.Update(product);
                txCtx.Commit();

                if (config.ProductStreaming)
                {
                    PriceUpdated update = new(
                         priceUpdate.sellerId,
                         priceUpdate.productId,
                         priceUpdate.price,
                         product.version,
                         priceUpdate.instanceId
                    );
                    await this.daprClient.PublishEventAsync(PUBSUB_NAME, nameof(PriceUpdated), update);
                }
            }
        }

        /*
         * Maybe use postgresql UPSERT: https://stackoverflow.com/questions/1109061/
         */
        public async Task ProcessCreateProduct(Product product)
        {
            using (var txCtx = this.dbContext.Database.BeginTransaction()) {
                ProductModel input = Utils.AsProductModel(product);
                input.created_at = DateTime.UtcNow;
                input.updated_at = input.created_at;
                this.productRepository.Insert(input);

                txCtx.Commit();
                if (config.ProductStreaming)
                {
                    await this.daprClient.PublishEventAsync(PUBSUB_NAME, nameof(Product), Utils.AsProduct(input));
                }
            }   
        }

        public async Task ProcessProductUpdate(Product product)
        {
            using (var txCtx = this.dbContext.Database.BeginTransaction())
            {
                var oldProduct = productRepository.GetProduct(product.seller_id, product.product_id);
                ProductModel input = Utils.AsProductModel(product);

                // keep the old
                input.created_at = oldProduct.created_at;

                this.productRepository.Update(input);

                txCtx.Commit();
                if (config.ProductStreaming)
                {
                    await this.daprClient.PublishEventAsync(PUBSUB_NAME, nameof(ProductUpdated), new ProductUpdated(input.seller_id, input.product_id, input.version));
                }
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