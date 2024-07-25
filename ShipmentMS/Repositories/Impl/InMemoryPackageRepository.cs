
using System.Collections.Concurrent;
using System.Data;
using Common.Entities;
using Microsoft.EntityFrameworkCore.Storage;
using ShipmentMS.Models;

namespace ShipmentMS.Repositories.Impl;

public class InMemoryPackageRepository : IPackageRepository
{
    private readonly ConcurrentDictionary<(int customerId, int orderId, int packageId),PackageModel> packages;

    private static readonly IDbContextTransaction DEFAULT_DB_TX = new NoTransactionScope();

	public InMemoryPackageRepository()
    { 
        this.packages = new();
	}

    public void Insert(PackageModel value)
    {
        this.packages.TryAdd((value.customer_id,value.order_id, value.package_id), value);
    }

    public void InsertAll(List<PackageModel> values)
    {
        foreach(var pkg in values)
        {
            this.Insert(pkg);
        }
    }

    public void Update(PackageModel newValue)
    {
        this.packages[(newValue.customer_id,newValue.order_id, newValue.package_id)] = newValue;
    }

    public void Delete((int, int, int) id)
    {
        this.packages.Remove(id, out _);
    }

    public PackageModel? GetById((int, int, int) id)
    {
        return this.packages[id];
    }

    public IDictionary<int, string[]> GetOldestOpenShipmentPerSeller()
    {
        return this.packages.Values
                .Where(x => x.status.Equals(PackageStatus.shipped))
                .GroupBy(x => x.seller_id)
                .Select(g => new { key = g.Key, Sort = g.Min(x => x.GetOrderIdAsString()) }).Take(10)
                .ToDictionary(g => g.key, g => g.Sort.Split("|"));
}

    public IEnumerable<PackageModel> GetShippedPackagesByOrderAndSeller(int customerId, int orderId, int sellerId)
    {
        return this.packages.Values.Where(p => p.customer_id == customerId && p.order_id == orderId && p.status == PackageStatus.shipped && p.seller_id == sellerId);
    }

    public int GetTotalDeliveredPackagesForOrder(int customerId, int orderId)
    {
        return this.packages.Values.Where(p => p.customer_id == customerId && p.order_id == orderId && p.status == PackageStatus.delivered).Count();
    }

    public void Save()
    {
        // do nothing
    }

    public IDbContextTransaction BeginTransaction(IsolationLevel isolationLevel)
    {
        return DEFAULT_DB_TX;
    }

    public void Cleanup()
    {
        this.packages.Clear();
    }

    public void Dispose()
    {
        // do nothing
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

