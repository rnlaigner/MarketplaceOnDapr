using System.ComponentModel.DataAnnotations.Schema;
using Common.Entities;
using Microsoft.EntityFrameworkCore;

namespace OrderMS.Common.Models
{
    [Table("orders", Schema = "order")]
    [PrimaryKey(nameof(id))]
    [Index(nameof(customer_id), IsUnique = false)]
    public class OrderModel
	{

        /*
         * Sequence does not block concurrent txs in PostgreSQL
         * but may leave holes if tx fails
         */
        public int id { get; set; }

        // https://finom.co/en-fr/blog/invoice-number/
        public string invoice_number { get; set; } = "";

        public int customer_id { get; set; }

        public OrderStatus status { get; set; } = OrderStatus.CREATED;

        public DateTime purchase_date { get; set; }

        public DateTime? payment_date { get; set; }

        public DateTime? delivered_carrier_date { get; set; }

        public DateTime? delivered_customer_date { get; set; }

        public DateTime? estimated_delivery_date { get; set; }

        public int count_items { get; set; }

        public DateTime created_at { get; set; }

        public DateTime updated_at { get; set; }

        public float total_amount { get; set; } = 0;
        public float total_freight { get; set; } = 0;
        public float total_incentive { get; set; } = 0;
        public float total_invoice { get; set; } = 0;
        public float total_items { get; set; } = 0;

        // only way to make migrations pick the relationship from the order item side?...
        [ForeignKey("order_id")]
        public ICollection<OrderItemModel> items { get; } = new List<OrderItemModel>();

        [ForeignKey("order_id")]
        public ICollection<OrderHistoryModel> history { get; } = new List<OrderHistoryModel>();

        public OrderModel() { }

	}
}

