using System;
using ShipmentMS.Models;

namespace ShipmentMS.Repositories
{
	public interface IPackageRepository : IRepository<(int,int), PackageModel>
	{
        IDictionary<int, int> GetOldestOpenShipmentPerSeller();

        IEnumerable<PackageModel> GetShippedPackagesByOrderAndSeller(int orderId, int sellerId);

        int GetTotalDeliveredPackagesForOrder(int orderId);

    }
}

