using Common.Entities;
using Common.Events;
using Common.Integration;
using Common.Requests;
using Dapr.Client;
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
                    return;
                }

                this.productRepository.Delete(product);
                txCtx.Commit();
                if (config.ProductStreaming)
                {
                    ProductUpdate productUpdate = new()
                    {
                        seller_id = productToDelete.sellerId,
                        product_id = productToDelete.productId,
                        price = product.price,
                        active = false,
                        instanceId = productToDelete.instanceId
                    };
                    await this.daprClient.PublishEventAsync(PUBSUB_NAME, nameof(ProductUpdate), productUpdate);
                }
            }
            
        }

        public async Task ProcessUpdate(UpdatePrice update)
        {
            using (var txCtx = this.dbContext.Database.BeginTransaction())
            {
                var product = this.productRepository.GetProduct(update.sellerId, update.productId);

                if (product is null)
                {
                    logger.LogWarning("[ProcessProductUpdate] Cannot find seller {0} product {1}.", update.sellerId, update.productId);
                    return;
                }

                product.updated_at = DateTime.Now;
                product.price = update.price;
                this.productRepository.Update(product);
                txCtx.Commit();

                if (config.ProductStreaming)
                {
                    ProductUpdate productUpdate = new()
                    {
                        seller_id = product.seller_id,
                        product_id = product.product_id,
                        price = product.price,
                        active = true,
                        instanceId = update.instanceId
                    };
                    await this.daprClient.PublishEventAsync(PUBSUB_NAME, nameof(ProductUpdate), productUpdate);
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

	}
}