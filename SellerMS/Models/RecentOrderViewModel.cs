using System;
namespace SellerMS.Models
{
	public class RecentOrderViewModel
    {
		public long seller_id { get; set; }

		// cache expiration directive. last 10 only

        public RecentOrderViewModel()
		{
		}
	}
}

