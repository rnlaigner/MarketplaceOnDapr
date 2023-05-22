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
    [Table("customer_orders")]
    [PrimaryKey(nameof(customer_id))]
    public class CustomerOrderModel
	{

        public string customer_id { get; set; }

        public long next_order_id { get; set; }

        public CustomerOrderModel() { }

    }
}

