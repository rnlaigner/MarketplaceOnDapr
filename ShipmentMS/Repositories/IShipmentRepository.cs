using System;
using ShipmentMS.Models;

namespace ShipmentMS.Repositories
{
	public interface IShipmentRepository : IRepository<long, ShipmentModel>
	{

		ShipmentModel? GetShipmentById(long orderId);

	}
}

