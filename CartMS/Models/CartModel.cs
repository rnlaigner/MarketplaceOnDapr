using System.ComponentModel.DataAnnotations.Schema;
using Common.Entities;
using Microsoft.EntityFrameworkCore;

namespace CartMS.Models
{
    [Table("carts", Schema = "cart")]
    [PrimaryKey(nameof(customer_id))]
    public class CartModel
	{
        public int customer_id { get; set; }

        public CartStatus status { get; set; } = CartStatus.OPEN;

        public DateTime created_at { get; set; }

        public DateTime updated_at { get; set; }

        [ForeignKey("customer_id")]
        public ICollection<CartItemModel> items { get; } = new List<CartItemModel>();

        public CartModel() {
            this.created_at = DateTime.UtcNow;
            this.updated_at = this.created_at;
        }
    }
}

