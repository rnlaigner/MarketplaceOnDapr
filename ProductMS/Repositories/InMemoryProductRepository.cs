using System.Collections.Concurrent;
using Microsoft.EntityFrameworkCore.Storage;
using ProductMS.Models;

namespace ProductMS.Repositories;

public class InMemoryProductRepository : IProductRepository
{

    private readonly ConcurrentDictionary<(int sellerId, int productId),ProductModel> products;

    private static readonly IDbContextTransaction DEFAULT_DB_TX = new NoTransactionScope();

	public InMemoryProductRepository()
	{
        this.products = new();
	}

    public IDbContextTransaction BeginTransaction()
    {
        return DEFAULT_DB_TX;
    }

    public IEnumerable<ProductModel> GetBySeller(int sellerId)
    {
        return this.products.Values.Where(i=>i.seller_id == sellerId);
    }

    public ProductModel? GetProduct(int sellerId, int productId)
    {
        if (this.products.ContainsKey((sellerId, productId)))
        {
            return this.products[(sellerId, productId)];
        }
        return null;
    }

    public ProductModel GetProductForUpdate(int sellerId, int productId)
    {
        var product = this.GetProduct(sellerId,productId);
        if(product is null) throw new ApplicationException($"Cannot find product {sellerId}-{productId}");
        return product;
    }

    public void Insert(ProductModel product)
    {
        product.created_at = DateTime.UtcNow;
        product.updated_at = product.created_at;
        product.active = true;
        this.products.TryAdd( (product.seller_id, product.product_id),  product);
    }

    public void Update(ProductModel product)
    {
        product.updated_at = DateTime.UtcNow;
        this.products[(product.seller_id, product.product_id)] = product;
    }

    public void Reset()
    {
        foreach(var item in this.products.Values)
        {
            item.active = true;
            item.version = "0";
        }
    }

    public void Cleanup()
    {
        this.products.Clear();
    }

    public class NoTransactionScope : IDbContextTransaction
    {
        public Guid TransactionId => throw new NotImplementedException();

        public void Commit()
        {
            // do nothing
        }

        public Task CommitAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            // do nothing
        }

        public ValueTask DisposeAsync()
        {
            throw new NotImplementedException();
        }

        public void Rollback()
        {
            throw new NotImplementedException();
        }

        public Task RollbackAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }

}

