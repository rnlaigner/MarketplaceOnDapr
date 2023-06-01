using System;
using Dapr.Client;
using ProductMS.Models;

namespace ProductMS.Repositories
{
	public class DaprProductRepository : IProductRepository
	{

        public const string StoreName = "statestore";

        private readonly DaprClient daprClient;

        private readonly ILogger<DaprProductRepository> logger;

        public DaprProductRepository(DaprClient daprClient, ILogger<DaprProductRepository> logger)
        {
            this.daprClient = daprClient;
            this.logger = logger;
        }

        public void Delete(ProductModel product)
        {
            throw new NotImplementedException();
        }

        public ProductModel? GetProduct(long sellerId, long productId)
        {
            throw new NotImplementedException();
        }

        public void Insert(ProductModel product)
        {
            throw new NotImplementedException();
        }

        public void Update(ProductModel product)
        {
            throw new NotImplementedException();
        }

        public ProductModel UpdatePrice(long productId, decimal newPrice)
        {
            throw new NotImplementedException();
        }
    }
}

