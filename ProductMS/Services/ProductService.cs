using Common.Entities;
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

        public async Task<bool> Delete(Product productToDelete)
        {
            try
            {
                using (var txCtx = this.dbContext.Database.BeginTransaction())
                {
                    ProductModel? product = this.productRepository.GetProduct(productToDelete.id);

                    if (product is null)
                    {
                        this.logger.LogWarning("Cannot find product id {0} to delete", productToDelete.id);
                        return false;
                    }
                    else
                    {
                        this.productRepository.Delete(product);
                    }

                    this.dbContext.SaveChanges();

                    productToDelete.active = false;

                    if(config.ProductStreaming)
                        await this.daprClient.PublishEventAsync(PUBSUB_NAME, nameof(Product), productToDelete);

                    txCtx.Commit();

                }
                return true;
            }
            catch(Exception e)
            {
                return false;
            }
        }

        /*
         * Maybe use postgresql UPSERT: https://stackoverflow.com/questions/1109061/
         */
        public async Task<bool> Upsert(Product productToUpdate)
        {
            try
            { 
                using (var txCtx = this.dbContext.Database.BeginTransaction()) {

                    ProductModel? product = this.productRepository.GetProduct(productToUpdate.id);
                    var product_ = Utils.AsProductModel(productToUpdate);

                    if (product is null)
                    {
                        this.productRepository.Insert(product_);
                    } else
                    {
                        this.productRepository.Update(product_);
                    }

                    this.dbContext.SaveChanges();

                    if (config.ProductStreaming) // TODO should we handle the commit exception with cancellation token?
                        await this.daprClient.PublishEventAsync(PUBSUB_NAME, nameof(Product), productToUpdate);
                
                    txCtx.Commit();

                }
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

	}
}

