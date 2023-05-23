using System;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using Common.Entities;

namespace SellerMS.Models
{

    [Table("order_entry_view")]
    [PrimaryKey(nameof(order_id))]
    [Index(nameof(seller_id))]
    public class OrderEntryViewModel
    {
        public long order_id { get; set; }

        public long seller_id { get; set; }

        public DateTime order_date { get; set; }

        public OrderStatus status { get; set; }

        // customer info

        // shipment info

        // payment info

        // cache expiration directive. last 10 only

        public OrderEntryViewModel()
		{
		}
	}
}

