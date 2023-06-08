using System;
using Google.Api;
using Google.Protobuf.WellKnownTypes;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using ShipmentMS.Infra;
using ShipmentMS.Models;

namespace ShipmentMS.Repositories
{
	public class ShipmentRepository : GenericRepository<long, ShipmentModel>, IShipmentRepository
	{

        public ShipmentRepository(ShipmentDbContext context) : base(context)
        {
        }

    }
}