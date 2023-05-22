using System;
namespace Common.Events
{
	public class FinancialReportRequest
	{
		public long sellerId { get; set; }
		public DateTime from { get; set; }
        public DateTime to { get; set; }

        public FinancialReportRequest()
		{
			this.from = DateTime.Today.AddDays(-1);
            this.to = DateTime.Today;
		}
	}
}

