using ShipmentMS.Infra;
using ShipmentMS.Models;

namespace ShipmentMS.Repositories
{
	public class ShipmentRepository : GenericRepository<int, ShipmentModel>, IShipmentRepository
	{

        public ShipmentRepository(ShipmentDbContext context) : base(context)
        {
        }

    }
}