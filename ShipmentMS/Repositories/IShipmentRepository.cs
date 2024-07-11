using ShipmentMS.Models;

namespace ShipmentMS.Repositories;

public interface IShipmentRepository : IRepository<(int,int), ShipmentModel>
{
    void Cleanup();
}
