using Common.Entities;
using ShipmentMS.Infra;
using ShipmentMS.Models;

namespace ShipmentMS.Repositories
{
	public class PackageRepository : GenericRepository<(long,int), PackageModel>, IPackageRepository
	{

        public PackageRepository(ShipmentDbContext context) : base(context)
		{
        }

        private const string query = "select seller_id, MIN(order_id) as custom from packages where packages.status == 'shipped' group by seller_id";

        public IDictionary<long, long> GetOldestOpenShipmentPerSeller()
        {
            return this.dbSet
                            .Where(x => x.status.Equals(PackageStatus.shipped.ToString()))
                            .GroupBy(x => x.seller_id)
                            .Select(g => new { key = g.Key, Sort = g.Min(x => x.order_id) })
                            .ToDictionary(g => g.key, g => g.Sort);
        }

        public IEnumerable<PackageModel> GetShippedPackagesByOrderAndSeller(long orderId, long sellerId)
        {
            return this.Get(p => p.status == PackageStatus.shipped && p.order_id == orderId);
        }

        public int GetTotalDeliveredPackagesForOrder(long orderId)
        {
            return this.dbSet.Where(p => p.status == PackageStatus.delivered && p.order_id == orderId).Count();
        }
    }
}

