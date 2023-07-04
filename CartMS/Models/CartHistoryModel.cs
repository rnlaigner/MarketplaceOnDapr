using System.ComponentModel.DataAnnotations.Schema;
using Common.Entities;
using Microsoft.EntityFrameworkCore;

/**
 * https://www.npgsql.org/efcore/mapping/json.html?tabs=data-annotations%2Cpoco
 */
namespace CartMS.Models
{
    [Table("cart_history")]
    [PrimaryKey(nameof(id))]
    public class CartHistoryModel
	{
        public long id { get; set; }
        public long customer_id { get; set; }

        public CartStatus status { get; set; }

        public DateTime created_at { get; set; }

        [Column(TypeName = "jsonb")]
        public List<CartItem>? items { get; set; }

        public CartHistoryModel() { }
    }
}