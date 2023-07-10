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

        public async Task ProcessDelete(DeleteProduct productToDelete)
        {
            using (var txCtx = this.dbContext.Database.BeginTransaction())
            {
                var product = this.productRepository.GetProduct(productToDelete.sellerId, productToDelete.productId);
                if(product is null)
                {
                    logger.LogWarning("[ProcessDelete] Cannot find seller {0} product {1}.", productToDelete.sellerId, productToDelete.productId);

                    if (config.ProductStreaming)
                    {
                        this.logger.LogInformation("Publishing transaction mark {0} to seller {1}", productToDelete.instanceId, productToDelete.productId);
                        string streamId = new StringBuilder(nameof(TransactionMark)).Append('_').Append(TransactionType.DELETE_PRODUCT.ToString()).ToString();
                        await this.daprClient.PublishEventAsync(PUBSUB_NAME, streamId, new TransactionMark(productToDelete.instanceId, TransactionType.DELETE_PRODUCT, productToDelete.sellerId));
                    }

                    return;
                }
                if (!product.active)
                {
                    // this should never happen. error in the driver!
                    logger.LogWarning("[ProcessDelete] Seller {0} product {1} has already been deleted!", productToDelete.sellerId, productToDelete.productId);

                    if (config.ProductStreaming)
                    {
                        this.logger.LogInformation("Publishing transaction mark {0} to seller {1}", productToDelete.instanceId, productToDelete.productId);
                        string streamId = new StringBuilder(nameof(TransactionMark)).Append('_').Append(TransactionType.DELETE_PRODUCT.ToString()).ToString();
                        await this.daprClient.PublishEventAsync(PUBSUB_NAME, streamId, new TransactionMark(productToDelete.instanceId, TransactionType.DELETE_PRODUCT, productToDelete.sellerId));
                    }

                    return;
                }

                this.productRepository.Delete(product);
                txCtx.Commit();
                if (config.ProductStreaming)
                {
                    ProductUpdate productUpdate = new(
                        productToDelete.sellerId,
                        productToDelete.productId,
                        product.price,
                        false,
                        productToDelete.instanceId
                    );
                    await this.daprClient.PublishEventAsync(PUBSUB_NAME, nameof(ProductUpdate), productUpdate);
                }
            }
            
        }

        public async Task ProcessUpdate(UpdatePrice priceUpdate)
        {
            using (var txCtx = this.dbContext.Database.BeginTransaction())
            {
                var product = this.productRepository.GetProduct(priceUpdate.sellerId, priceUpdate.productId);

                if (product is null)
                {
                    logger.LogWarning("[ProcessProductUpdate] Cannot find seller {0} product {1}.", priceUpdate.sellerId, priceUpdate.productId);

                    if (config.ProductStreaming)
                    {
                        this.logger.LogInformation("Publishing transaction mark {0} to seller {1}", priceUpdate.instanceId, priceUpdate.sellerId);
                        string streamId = new StringBuilder(nameof(TransactionMark)).Append('_').Append(TransactionType.PRICE_UPDATE.ToString()).ToString();
                        await this.daprClient.PublishEventAsync(PUBSUB_NAME, streamId, new TransactionMark(priceUpdate.instanceId, TransactionType.PRICE_UPDATE, priceUpdate.sellerId));
                    }

                    return;
                }
                if (!product.active)
                {
                    logger.LogWarning("[ProcessUpdate] Seller {0} product {1} has been deleted. Cannot process update!", priceUpdate.sellerId, priceUpdate.productId);

                    if (config.ProductStreaming)
                    {
                        this.logger.LogInformation("Publishing transaction mark {0} to seller {1}", priceUpdate.instanceId, priceUpdate.sellerId);
                        string streamId = new StringBuilder(nameof(TransactionMark)).Append('_').Append(TransactionType.PRICE_UPDATE.ToString()).ToString();
                        await this.daprClient.PublishEventAsync(PUBSUB_NAME, streamId, new TransactionMark(priceUpdate.instanceId, TransactionType.PRICE_UPDATE, priceUpdate.sellerId));
                    }

                    return;
                }

                product.updated_at = DateTime.Now;
                product.price = priceUpdate.price;
                this.productRepository.Update(product);
                txCtx.Commit();

                if (config.ProductStreaming)
                {
                    ProductUpdate update = new(
                         priceUpdate.sellerId,
                         priceUpdate.productId,
                         priceUpdate.price,
                         true,
                         priceUpdate.instanceId
                    );
                    await this.daprClient.PublishEventAsync(PUBSUB_NAME, nameof(ProductUpdate), update);
                }
            }
        }

        /*
         * Maybe use postgresql UPSERT: https://stackoverflow.com/questions/1109061/
         */
        public async Task ProcessNewProduct(Product productToUpdate)
        {
            using (var txCtx = this.dbContext.Database.BeginTransaction()) {
                ProductModel input = Utils.AsProductModel(productToUpdate);
                input.created_at = DateTime.Now;
                input.updated_at = input.created_at;
                this.productRepository.Insert(input);

                txCtx.Commit();
                if (config.ProductStreaming)
                {
                    await this.daprClient.PublishEventAsync(PUBSUB_NAME, nameof(Product), Utils.AsProduct(input));
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