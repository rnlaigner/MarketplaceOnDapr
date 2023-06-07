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

        public DateTime created_at { get; set; } = DateTime.Now;

        public DateTime updated_at { get; set; } = DateTime.Now;

        [ForeignKey("customer_id")]
        public ICollection<CartItemModel> packages { get; } = new List<CartItemModel>();

        public CartModel() { }
    }
}

