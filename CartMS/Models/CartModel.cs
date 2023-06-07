using System;
using System.ComponentModel.DataAnnotations.Schema;
using Common.Entities;
using Microsoft.EntityFrameworkCore;

namespace CartMS.Models
{
    [Table("carts")]
    [PrimaryKey(nameof(customer_id))]
    public class CartModel
	{
        public long customer_id { get; set; }

        public CartStatus status { get; set; } = CartStatus.OPEN;

        // public IDictionary<long, CartItem> items { get; set; };

        public DateTime created_at { get; set; } = DateTime.Now;

        public DateTime updated_at { get; set; } = DateTime.Now;

        // public IList<CartItemModel> items = new List<CartItemModel>();

        public CartModel() { }
    }
}

