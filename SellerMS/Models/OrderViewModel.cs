using System;
namespace SellerMS.Models
{
	public class OrderViewModel
	{
		public long seller_id { get; set; }

        public decimal total_overall { get; set; }
		public decimal revenue { get; set; }
		public int count_orders { get; set; }
		public decimal avg_order_value { get; set; }
		public decimal avg_order_revenue { get; set; }

        public OrderViewModel()
		{
		}
	}
}

