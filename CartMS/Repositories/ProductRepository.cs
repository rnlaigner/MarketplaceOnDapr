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

        public Task<bool> Delete(Product product)
        {
            throw new NotImplementedException();
        }

        public async Task<Product> GetProduct(string id)
        {
            return await daprClient.GetStateAsync<Product>(StoreName, id, ConsistencyMode.Strong);
        }

        // private static readonly List<String> emptyList = new (){};

        public async Task<IList<Product>> GetProducts(IReadOnlyList<string> ids)
        {
            IReadOnlyList<BulkStateItem> mulitpleStateResult = await daprClient.GetBulkStateAsync(StoreName, ids, parallelism: 1);
            return (IList<Product>)mulitpleStateResult.Select(b => b.Value).ToList();
        }

        public async Task<bool> Upsert(Product product)
        {
            await daprClient.SaveStateAsync(StoreName, product.sku, product);
            return true;
        }
    }
}

