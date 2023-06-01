using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection.Metadata;
using Microsoft.EntityFrameworkCore;

/*
 * More info in https://learn.microsoft.com/en-us/ef/core/modeling/relationships/one-to-many
 * 
 */
namespace OrderMS.Common.Models
{
    [Table("order_items")]
    [PrimaryKey(nameof(order_id), nameof(order_item_id))]
    public class OrderItemModel
	{

        /* another way (must remove the foreign key in ordermodel):
        [ForeignKey("order")]
        public long order_id { get; set; }
        public OrderModel order { get; set; }
        */
        public long order_id { get; set; }

        public long order_item_id { get; set; }

        public long product_id { get; set; }

        public string product_name { get; set; } = "";

        public long seller_id { get; set; }

        public decimal unit_price { get; set; }

        public DateTime shipping_limit_date { get; set; }

        public decimal freight_value { get; set; }

        // not present in olist
        public int quantity { get; set; }

        // without freight value
        public decimal total_items { get; set; }

        // without freight value
        public decimal total_amount { get; set; }

        // can be derived from total_items - total_amount
        // incentive of item is not of concern to the order
        // the seller must compute 
        // public decimal total_incentive { get; set; }

        public OrderItemModel() { }

    }
}

