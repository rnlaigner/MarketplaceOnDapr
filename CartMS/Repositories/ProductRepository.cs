using System;
using Common.Entities;
using Dapr.Client;

namespace CartMS.Repositories
{
    /**
     * Represents the repository of replicated products
     * The format P|{productId} is used to differentiate 
     * from possible clashes with the cart, that uses
     * customerId as key
     */
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
            string id = string.Format("P|{0}", product.product_id);
            Task task = daprClient.DeleteStateAsync(StoreName, id);
            await task;
            return task.IsCompletedSuccessfully;
        }

        public async Task<Product> GetProduct(long productId)
        {
            string id = string.Format("P|{0}", productId);
            return await daprClient.GetStateAsync<Product>(StoreName, id);
        }

        // private static readonly List<String> emptyList = new (){};

        public async Task<IList<Product>> GetProducts(IReadOnlyList<long> productIds)
        {
            var parsedInput = productIds.Select(id => string.Format("P|{0}", id)).ToList();
            IReadOnlyList<BulkStateItem> mulitpleStateResult = await daprClient.GetBulkStateAsync(StoreName, parsedInput, parallelism: 1);
            return (IList<Product>)mulitpleStateResult.Select(b => b.Value).ToList();
        }

        public async Task<bool> Upsert(Product product)
        {
            string id = string.Format("P|{0}", product.product_id);
            Task task = daprClient.SaveStateAsync(StoreName, id, product);
            await task;
            return task.IsCompletedSuccessfully;
        }
    }
}

