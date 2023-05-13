using System;
using ShipmentMS.Models;

namespace ShipmentMS.Repositories
{
	public interface IPackageRepository : IRepository<(long,int), PackageModel>
	{

	}
}

