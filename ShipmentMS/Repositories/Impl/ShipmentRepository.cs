using Microsoft.EntityFrameworkCore;
using ShipmentMS.Infra;
using ShipmentMS.Models;

namespace ShipmentMS.Repositories.Impl;

public class ShipmentRepository : GenericRepository<(int,int), ShipmentModel>, IShipmentRepository
{

    public ShipmentRepository(ShipmentDbContext context) : base(context)
    {
    }

    public void Cleanup()
    {
        this.dbSet.ExecuteDelete();
        this.context.SaveChanges();
    }
}
