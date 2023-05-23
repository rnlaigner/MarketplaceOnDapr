using System;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace SellerMS.Models
{
    [Table("shipment_view")]
    [PrimaryKey(nameof(seller_id))]
    public class ShipmentViewModel
	{
		public long seller_id { get; set; }

        public decimal total_number { get; set; }
		public long avg_mean_time_to_complete { get; set; }
		public decimal avg_shipment_value_per_order { get; set; }
		public decimal avg_shipment_value_per_item { get; set; }

		// avg lateness per order and item?

		// payment. total used payment method. card, boleto, coupon, debit credit

        public ShipmentViewModel()
		{
		}
	}
}

