using System;
using Google.Api;
using Google.Protobuf.WellKnownTypes;
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

        /*
         * Could be configureed dynamically to save or not the changes here.. injection on constructor
         */


        /*
        public void Delete(long id)
        {
            var v = this.GetByID(id);
            if (v is not null) context.Remove(v);
        }



        public IEnumerable<ShipmentModel> Get()
        {
            return context.Shipments;
        }

        public ShipmentModel? GetByID(long id)
        {
            return context.Shipments.Where(c => c.order_id == id).FirstOrDefault();
        }

        public void Insert(ShipmentModel value)
        {
            context.Shipments.Add(value);
        }

        public void Save()
        {
            context.SaveChanges();
        }

        public void Update(ShipmentModel newValue)
        {
            context.Shipments.Update(newValue);
        }
        */
    }
}

