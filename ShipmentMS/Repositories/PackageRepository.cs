using Common.Entities;
using ShipmentMS.Infra;
using ShipmentMS.Models;

namespace ShipmentMS.Repositories
{
	public class PackageRepository : GenericRepository<(int,int), PackageModel>, IPackageRepository
	{

        public PackageRepository(ShipmentDbContext context) : base(context)
		{
        }

        public IDictionary<int, int> GetOldestOpenShipmentPerSeller()
        {
            return this.dbSet
                            .Where(x => x.status.Equals(PackageStatus.shipped))
                            .GroupBy(x => x.seller_id)
                            .Select(g => new { key = g.Key, Sort = g.Min(x => x.order_id) }).Take(10)
                            .ToDictionary(g => g.key, g => g.Sort);
        }

        public IEnumerable<PackageModel> GetShippedPackagesByOrderAndSeller(int orderId, int sellerId)
        {
            return this.dbSet.Where(p => p.order_id == orderId && p.status == PackageStatus.shipped && p.seller_id == sellerId);
        }

        public int GetTotalDeliveredPackagesForOrder(int orderId)
        {
            return this.dbSet.Where(p => p.order_id == orderId && p.status == PackageStatus.delivered).Count();
        }
    }
}

