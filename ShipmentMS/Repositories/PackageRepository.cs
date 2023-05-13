using System;
using Google.Api;
using Google.Protobuf.WellKnownTypes;
using Microsoft.EntityFrameworkCore;
using ShipmentMS.Infra;
using ShipmentMS.Models;

namespace ShipmentMS.Repositories
{
	public class PackageRepository : GenericRepository<(long,int), PackageModel>, IPackageRepository
	{

        public PackageRepository(ShipmentDbContext context) : base(context)
		{
        }

    }
}

