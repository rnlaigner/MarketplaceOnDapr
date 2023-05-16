using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Common.Entities;
using Common.Events;
using Microsoft.EntityFrameworkCore;

namespace OrderMS.Common.Models
{
    [Table("orders")]
    [PrimaryKey(nameof(id))]
    public class OrderModel
	{

        /*
         * Sequence does not block concurrent txs in PostgreSQL
         * but may leave holes if tx fails
         */
        public long id { get; set; }

        public string customer_id { get; set; }

        public OrderStatus status { get; set; } = OrderStatus.CREATED;

        public DateTime purchase_date { get; set; }

        public DateTime? payment_date { get; set; }

        public DateTime? delivered_carrier_date { get; set; }

        public DateTime? delivered_customer_date { get; set; }

        public DateTime? estimated_delivery_date { get; set; }

        public int count_items { get; set; }

        public int count_delivered_items { get; set; }

        public DateTime created_at { get; set; } = DateTime.Now;

        public DateTime? updated_at { get; set; }

        public decimal total_amount { get; set; } = 0;
        public decimal total_freight { get; set; } = 0;
        public decimal total_incentive { get; set; } = 0;
        public decimal total_invoice { get; set; } = 0;
        public decimal total_items { get; set; } = 0;

        // for workflow
        public string instanceId { get; set; }

        // only way to make migrations pick the relationship from the order item side?...
        [ForeignKey("order_id")]
        public ICollection<OrderItemModel> items { get; } = new List<OrderItemModel>();

        [ForeignKey("order_id")]
        public ICollection<OrderHistoryModel> history { get; } = new List<OrderHistoryModel>();

        public OrderModel() { }

	}
}

