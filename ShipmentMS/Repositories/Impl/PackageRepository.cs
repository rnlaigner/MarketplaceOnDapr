using Common.Entities;
using Microsoft.EntityFrameworkCore;
using ShipmentMS.Infra;
using ShipmentMS.Models;

namespace ShipmentMS.Repositories.Impl;

public class PackageRepository : GenericRepository<(int,int,int), PackageModel>, IPackageRepository
{

    public PackageRepository(ShipmentDbContext context) : base(context)
	{
    }

    public IDictionary<int, string[]> GetOldestOpenShipmentPerSeller()
    {
        return this.dbSet
                        .Where(x => x.status.Equals(PackageStatus.shipped))
                        .GroupBy(x => x.seller_id)
                        .Select(g => new { key = g.Key, Sort = g.Min(x => x.GetOrderIdAsString()) }).Take(10)
                        .ToDictionary(g => g.key, g => g.Sort.Split("|"));
    }

    public IEnumerable<PackageModel> GetShippedPackagesByOrderAndSeller(int customerId, int orderId, int sellerId)
    {
        return this.dbSet.Where(p => p.customer_id == customerId && p.order_id == orderId && p.status == PackageStatus.shipped && p.seller_id == sellerId);
    }

    public int GetTotalDeliveredPackagesForOrder(int customerId, int orderId)
    {
        return this.dbSet.Where(p => p.customer_id == customerId && p.order_id == orderId && p.status == PackageStatus.delivered).Count();
    }

    public void Cleanup()
    {
        this.dbSet.ExecuteDelete();
        this.context.SaveChanges();
    }
}

