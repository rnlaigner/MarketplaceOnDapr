using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace CartMS.Models
{
    [Table("cart_items")]
    [PrimaryKey(nameof(customer_id), nameof(seller_id), nameof(product_id))]
    public class CartItemModel
	{
        public long customer_id { get; set; }

        public long seller_id { get; set; }

        public long product_id { get; set; }

        public string product_name { get; set; } = "";

        public decimal unit_price { get; set; }

        public decimal freight_value { get; set; }

        public int quantity { get; set; }

        public string? vouchers { get; set; }

        public CartItemModel() { }
    }
}

