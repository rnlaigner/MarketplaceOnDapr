using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace SellerMS.Models
{

    public class OrderView
	{
		public long seller_id { get; set; }

        public int count_orders { get; set; }

        public decimal total_overall { get; set; }
		public decimal revenue { get; set; }
		
		public decimal avg_order_value { get; set; }
		public decimal avg_order_revenue { get; set; }

        public OrderView()
		{
		}
	}

}