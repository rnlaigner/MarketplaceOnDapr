using System.Collections.Concurrent;
using Microsoft.EntityFrameworkCore.Storage;
using StockMS.Models;

namespace StockMS.Repositories;

public class InMemoryStockRepository : IStockRepository
{
    private readonly ConcurrentDictionary<(int sellerId, int productId),StockItemModel> stockItems;

    private static readonly IDbContextTransaction DEFAULT_DB_TX = new NoTransactionScope();

	public InMemoryStockRepository()
	{
        this.stockItems = new();
	}

    public IDbContextTransaction BeginTransaction()
    {
        return DEFAULT_DB_TX;
    }

    public StockItemModel? Find(int sellerId, int productId)
    {
        if (this.stockItems.ContainsKey((sellerId, productId)))
        {
            return this.stockItems[(sellerId, productId)];
        }
        return null;
    }

    public StockItemModel FindForUpdate(int sellerId, int productId)
    {
        var item = this.Find(sellerId, productId);
        if(item is null) throw new ApplicationException($"Cannot find stock item {sellerId}-{productId}");
        return item;
    }

    public void FlushUpdates()
    {
        // do nothing
    }

    public IEnumerable<StockItemModel> GetAll()
    {
        return this.stockItems.Values;
    }

    public IEnumerable<StockItemModel> GetBySellerId(int sellerId)
    {
        return this.stockItems.Values.Where(i=>i.seller_id == sellerId);
    }

    public IEnumerable<StockItemModel> GetItems(List<(int SellerId, int ProductId)> ids)
    {
        List<StockItemModel> list = new List<StockItemModel>();
        foreach(var i in ids)
        {
            list.Add( this.stockItems[(i.SellerId,i.ProductId)] );
        }
        return list;
    }

    public StockItemModel Insert(StockItemModel item)
    {
        this.stockItems.TryAdd((item.seller_id, item.product_id), item);
        return item;
    }

    public void Update(StockItemModel item)
    {
         item.updated_at = DateTime.UtcNow;
         this.stockItems[(item.seller_id, item.product_id)] = item;
    }

    public void UpdateRange(List<StockItemModel> stockItemsReserved)
    {
        foreach(var item in stockItemsReserved)
        {
            this.Update(item);
        }
    }

    public void Cleanup()
    {
        this.stockItems.Clear();
    }

    public void Reset(int qty)
    {
        foreach(var item in this.stockItems.Values)
        {
            item.qty_available = qty;
            item.version = "0";
            item.qty_reserved = 0;
        }
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

