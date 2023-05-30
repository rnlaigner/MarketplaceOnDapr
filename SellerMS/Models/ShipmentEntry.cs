using Common.Entities;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace SellerMS.Models
{
    /*
     * Represents both an order line and a shipment
     * Merge order line and delivery. good for the seller to have it all merged.
     */
    [Table("shipment_entries")]
    [PrimaryKey(nameof(seller_id))]
    public class ShipmentEntry
    {
		public long seller_id { get; set; }
        public long order_id { get; set; }
        public long package_id { get; set; }

        public long product_id { get; set; }
        public string product_name { get; set; }

        public int quantity { get; set; }

        public decimal total_amount { get; set; } = 0;
        public decimal freight_value { get; set; }

        public DateTime shipment_date { get; set; }

        public DateTime delivery_date { get; set; }

        public PackageStatus status { get; set; }

        public ShipmentEntry()
		{
		}
	}
}

