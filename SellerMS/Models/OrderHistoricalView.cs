using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace SellerMS.Models
{

    /**
	 * Materialized view
	 * Historical overview of orders
	 * 
	 */
    public class OrderHistoricalView
	{
		public long seller_id { get; set; }

        public int count_orders { get; set; }

        public decimal total_overall { get; set; }
		public decimal revenue { get; set; }
		
		public decimal avg_order_value { get; set; }
		public decimal avg_order_revenue { get; set; }

        public OrderHistoricalView()
		{
		}
	}

}