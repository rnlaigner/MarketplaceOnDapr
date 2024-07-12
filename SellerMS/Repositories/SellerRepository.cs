using Common.Entities;
using Common.Events;
using SellerMS.Infra;
using SellerMS.Models;
using SellerMS.DTO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace SellerMS.Repositories;

public class SellerRepository : ISellerRepository
{
    private readonly SellerDbContext dbContext;

    private readonly ILogger<SellerRepository> logger;

    public SellerRepository(SellerDbContext sellerDbContext, ILogger<SellerRepository> logger)
    {
        this.dbContext = sellerDbContext;
        this.logger = logger;
    }

    public SellerModel Insert(SellerModel seller)
    {
        var entity = this.dbContext.Sellers.Add(seller).Entity;
        this.dbContext.SaveChanges();
        return entity;
    }

    public void AddOrderEntry(OrderEntry orderEntry)
    {
        this.dbContext.OrderEntries.Add(orderEntry);
    }

    public void Update(OrderEntry orderEntry)
    {
        this.dbContext.OrderEntries.Update(orderEntry);
    }

    public void UpdateRange(IEnumerable<OrderEntry> orderEntries)
    {
        this.dbContext.OrderEntries.UpdateRange(orderEntries);
    }

    public SellerModel? Get(int sellerId)
    {
        return dbContext.Sellers.Find(sellerId); 
    }

    public IEnumerable<OrderEntry> GetOrderEntries(int customerId, int orderId)
    {
        return this.dbContext.OrderEntries.Where(oe =>
                oe.customer_id == customerId &&
                oe.order_id == orderId).AsNoTracking();
    }

    public OrderEntry? Find(int customerId, int orderId, int sellerId, int productId)
    {
        var oe = this.dbContext.OrderEntries.Find(
                customerId, 
                orderId,
                sellerId,
                productId);
        if(oe is not null) return oe;
        return null;
    }

    public SellerDashboard QueryDashboard(int sellerId)
    {
        return new SellerDashboard(
            this.dbContext.OrderSellerView.Where(v => v.seller_id == sellerId).AsEnumerable().FirstOrDefault(new OrderSellerView()),
            this.dbContext.OrderEntries.Where(oe => oe.seller_id == sellerId && (oe.order_status == OrderStatus.INVOICED || oe.order_status == OrderStatus.READY_FOR_SHIPMENT ||
                                                            oe.order_status == OrderStatus.IN_TRANSIT || oe.order_status == OrderStatus.PAYMENT_PROCESSED)).ToList()
            );
    }
    
    private static int LOCKED = 0;

    // this method allows a natural accumulation of concurrent requests
    // thus decreasing overall cost of updating seller view
    public void RefreshSellerViewSafely()
    {
        if (0 == Interlocked.CompareExchange(ref LOCKED, 1, 0))
        {
            this.dbContext.Database.ExecuteSqlRaw(SellerDbContext.ORDER_SELLER_VIEW_UPDATE_SQL);
            Interlocked.Exchange(ref LOCKED, 0);
        }
    }

    public IDbContextTransaction BeginTransaction()
    {
        return this.dbContext.Database.BeginTransaction();
    }

    public void FlushUpdates()
    {
        this.dbContext.SaveChanges();
    }

    // cleanup cleans up the database
    public void Cleanup()
    {
        this.dbContext.Sellers.ExecuteDelete();
        this.dbContext.OrderEntries.ExecuteDelete();
        this.dbContext.SaveChanges();
        this.dbContext.Database.ExecuteSqlRaw(SellerDbContext.ORDER_SELLER_VIEW_UPDATE_SQL);
    }

    // reset maintains seller records
    public void Reset()
    {
        this.dbContext.OrderEntries.ExecuteDelete();
        this.dbContext.SaveChanges();
        this.dbContext.Database.ExecuteSqlRaw(SellerDbContext.ORDER_SELLER_VIEW_UPDATE_SQL);
    }
    
}

