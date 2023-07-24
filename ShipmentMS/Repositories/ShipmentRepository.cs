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