using System;
using Common.Entities;
using Dapr.Client;

namespace CartMS.Repositories
{
    public class ProductRepository : IProductRepository
    {
        public const string StoreName = "statestore";

        private readonly DaprClient daprClient;

        private readonly ILogger<ProductRepository> logger;

        public ProductRepository(DaprClient daprClient, ILogger<ProductRepository> logger)
        {
            this.daprClient = daprClient;
            this.logger = logger;
        }

        public async Task<bool> Delete(Product product)
        {
            Task task = daprClient.DeleteStateAsync(StoreName, product.sku);
            await task;
            return task.IsCompletedSuccessfully;
        }

        public async Task<Product> GetProduct(string sku)
        {
            return await daprClient.GetStateAsync<Product>(StoreName, sku);
        }

        // private static readonly List<String> emptyList = new (){};

        public async Task<IList<Product>> GetProducts(IReadOnlyList<string> skus)
        {
            IReadOnlyList<BulkStateItem> mulitpleStateResult = await daprClient.GetBulkStateAsync(StoreName, skus, parallelism: 1);
            return (IList<Product>)mulitpleStateResult.Select(b => b.Value).ToList();
        }

        public async Task<bool> Upsert(Product product)
        {
            Task task = daprClient.SaveStateAsync(StoreName, product.sku, product);
            await task;
            return task.IsCompletedSuccessfully;
        }
    }
}

