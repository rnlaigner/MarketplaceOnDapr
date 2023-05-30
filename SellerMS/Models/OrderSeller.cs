using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace SellerMS.Models
{

    [Table("order_sellers")]
    [PrimaryKey(nameof(order_id),nameof(seller_id))]
    [Index(nameof(seller_id))]
    public class OrderSeller
    {
        public long order_id { get; set; }

        public long seller_id { get; set; }

        // information below from a seller's perspective

        // order
        public int count_items { get; set; }
        public decimal total_amount { get; set; } = 0;
        public decimal total_freight { get; set; } = 0;

        public decimal total_incentive { get; set; } = 0;

        public decimal total_invoice { get; set; } = 0;
        public decimal total_items { get; set; } = 0;

        // from down below, all the same. could be normalized.... e.g., order_details table, shared across sellers
        [ForeignKey("order_id")]
        public OrderDetails details { get; set; }

        public OrderSeller()
		{
		}
	}
}

