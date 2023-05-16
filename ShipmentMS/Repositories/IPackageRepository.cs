using System;
using ShipmentMS.Models;

namespace ShipmentMS.Repositories
{
	public interface IPackageRepository : IRepository<(long,int), PackageModel>
	{
        IDictionary<long, long> GetOldestOpenShipmentPerSeller();

        IEnumerable<PackageModel> GetShippedPackagesByOrderAndSeller(long orderId, long sellerId);

        int GetTotalDeliveredPackagesForOrder(long orderId);

    }
}

