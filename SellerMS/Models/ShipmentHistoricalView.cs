using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace SellerMS.Models
{

	/**
	 * Materialized view
	 * Historical overview of shipments
	 */
    public class ShipmentHistoricalView
	{
		public long seller_id { get; set; }

        public decimal count_shipments { get; set; }
		public long avg_time_to_complete { get; set; }
		public decimal avg_freight_value { get; set; }

		public decimal total_freight_amount { get; set; }
        // avg lateness per order and item?

        public ShipmentHistoricalView()
		{
		}
	}
}

