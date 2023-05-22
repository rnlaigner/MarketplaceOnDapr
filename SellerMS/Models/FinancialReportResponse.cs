using System;
namespace SellerMS.Models
{
	public class FinancialReportResponse
	{
		public long id;
		public long seller_id;

		public DateTime from { get; set; }
        public DateTime to { get; set; }

        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }

		// the seller ms works works as an on demand query service
		// old reports are purged by an internal task to avoid overgrown data

		// can sellers register periodic sending of reports?

        public FinancialReportResponse()
		{
		}
	}
}

