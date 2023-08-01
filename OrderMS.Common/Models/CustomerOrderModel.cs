using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

/*
 * More info in https://learn.microsoft.com/en-us/ef/core/modeling/relationships/one-to-many
 * 
 */
namespace OrderMS.Common.Models
{
    [Table("customer_orders", Schema = "order")]
    [PrimaryKey(nameof(customer_id))]
    public class CustomerOrderModel
	{

        public int customer_id { get; set; }

        public int next_order_id { get; set; }

        public CustomerOrderModel() { }

    }
}

