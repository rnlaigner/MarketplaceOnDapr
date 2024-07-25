using System.Collections.Concurrent;
using System.Data;
using Microsoft.EntityFrameworkCore.Storage;
using ShipmentMS.Models;

namespace ShipmentMS.Repositories.Impl;

public class InMemoryShipmentRepository : IShipmentRepository
{
    private readonly ConcurrentDictionary<(int customerId, int orderId),ShipmentModel> shipments;

    private static readonly IDbContextTransaction DEFAULT_DB_TX = new NoTransactionScope();

	public InMemoryShipmentRepository()
	{
        this.shipments = new();
	}

    public void Insert(ShipmentModel value)
    {
        this.shipments.TryAdd((value.customer_id,value.order_id), value);
    }

    public void InsertAll(List<ShipmentModel> values)
    {
        foreach(var ship in values)
        {
            this.Insert(ship);
        }
    }

    public void Update(ShipmentModel newValue)
    {
        this.shipments[(newValue.customer_id,newValue.order_id)] = newValue;
    }

    public void Delete((int,int) id)
    {
        this.shipments.Remove(id, out _);
    }

    public ShipmentModel? GetById((int,int) id)
    {
        return this.shipments[id];
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
        this.shipments.Clear();
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
