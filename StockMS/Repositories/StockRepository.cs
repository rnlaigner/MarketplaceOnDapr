using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using StockMS.Infra;
using StockMS.Models;

namespace StockMS.Repositories;

public class StockRepository : IStockRepository
{
    private readonly StockDbContext dbContext;
    private readonly ILogger<StockRepository> logger;

    public StockRepository(StockDbContext stockDbContext, ILogger<StockRepository> logger)
	{
        this.dbContext = stockDbContext;
        this.logger = logger;
	}

    public StockItemModel Insert(StockItemModel item)
    {
        item.active = true;
        item.created_at = DateTime.Now;
        item.updated_at = item.created_at;
        return this.dbContext.StockItems.Add(item).Entity;
    }

    public void Update(StockItemModel item)
    {
        item.updated_at = DateTime.UtcNow;
        this.dbContext.StockItems.Update(item);
        this.dbContext.SaveChanges();
    }

    public void UpdateRange(List<StockItemModel> stockItemsReserved)
    {
        this.dbContext.StockItems.UpdateRange(stockItemsReserved);
    }

    public IEnumerable<StockItemModel> GetAll()
    {
        return this.dbContext.StockItems;
    }

    // https://www.postgresql.org/docs/current/explicit-locking.html#LOCKING-ROWS
    private const string sqlGetItemsForUpdate = "SELECT * FROM stock.stock_items s WHERE (s.seller_id, s.product_id) IN ({0}) order by s.seller_id, s.product_id FOR UPDATE";

    public IEnumerable<StockItemModel> GetItems(List<(int SellerId, int ProductId)> ids)
    {
        var sb = new StringBuilder();
        foreach (var (SellerId, ProductId) in ids)
        {
            sb.Append('(').Append(SellerId).Append(',').Append(ProductId).Append("),");
        }
        var input = sb.Remove(sb.Length - 1,1).ToString();
        logger.LogDebug("SQL input is {0}", input);
        var sql = string.Format(sqlGetItemsForUpdate, input);
        logger.LogDebug("SQL is {0}", sql);
        return dbContext.StockItems.FromSqlRaw(sql);
    }

    public StockItemModel? Find(int sellerId, int productId)
    {
        return this.dbContext.StockItems.Find(sellerId, productId);
    }

    private const string SELECT_ITEM_FOR_UPDATE = "SELECT * FROM stock.stock_items s WHERE s.seller_id = {0} AND s.product_id = {1} FOR UPDATE";

    public StockItemModel FindForUpdate(int sellerId, int productId)
    {
        var sql = string.Format(SELECT_ITEM_FOR_UPDATE, sellerId, productId);
        // this will fail if the item is not found
        // return this.dbContext.StockItems.FromSqlRaw(sql).First();
        try
        {
            return this.dbContext.StockItems.FromSqlRaw(sql).First();
            // this.logger.LogWarning($"Item {sellerId}-{productId} locked");
            // return item;
        } catch(Exception e)
        {
            logger.LogCritical($"Item {sellerId}-{productId} cannot be found or locked");
            throw new ApplicationException(e.ToString());
        }
    }

    public IEnumerable<StockItemModel> GetBySellerId(int sellerId)
    {
        return this.dbContext.StockItems.Where(p => p.seller_id == sellerId);
    }

    public IDbContextTransaction BeginTransaction()
    {
        return this.dbContext.Database.BeginTransaction();
    }

    public void FlushUpdates()
    {
        this.dbContext.SaveChanges();
    }

    public void Cleanup()
    {
        this.dbContext.StockItems.ExecuteDelete();
        this.dbContext.SaveChanges();
    }

    public void Reset(int qty)
    {
        this.dbContext.Database.ExecuteSqlRaw(string.Format("UPDATE stock.stock_items SET active=true, version='0', qty_reserved=0, qty_available={0}", qty));
        this.dbContext.SaveChanges();
    }

}

