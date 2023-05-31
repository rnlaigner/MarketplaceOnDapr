using Common.Entities;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace SellerMS.Models
{
    /*
     * Represents both an order line and a shipment.
     * It is a denormalized representation of several tables.
     * The fact table for order historical view.
     */

    [Table("order_entries")]
    [PrimaryKey(nameof(order_id), nameof(product_id))]
    [Index(nameof(seller_id))]
    public class OrderEntry
    {
		public long seller_id { get; set; }
        public long order_id { get; set; }
        public long? package_id { get; set; }

        public long product_id { get; set; }
        public string product_name { get; set; } = "";
        public string product_category { get; set; } = "";

        public decimal unit_price { get; set; }
        public int quantity { get; set; }

        public decimal total_items { get; set; }
        public decimal total_amount { get; set; }
        public decimal total_incentive { get; set; }
        public decimal total_invoice { get; set; } = 0;
        public decimal freight_value { get; set; }

        public DateTime? shipment_date { get; set; }

        public DateTime? delivery_date { get; set; }

        // denormalized, thus redundant. to avoid join on details
        public OrderStatus order_status { get; set; }

        public PackageStatus? delivery_status { get; set; }

        // from down below, all the same. could be normalized.... e.g., order_details table, shared across sellers
        [ForeignKey("order_id")]
        public OrderEntryDetails details { get; set; }

        public OrderEntry()
		{
		}
	}
}

