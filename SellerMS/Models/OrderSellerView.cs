namespace SellerMS.Models
{

    /**
     * View
     * Overview of orders in progress
     * 
     */
    public class OrderSellerView
    {
        public long seller_id { get; set; }

        // information below from a seller's perspective

        // order
        public int count_items { get; set; } = 0;
        public decimal total_amount { get; set; } = 0;
        public decimal total_freight { get; set; } = 0;

        public decimal total_incentive { get; set; } = 0;

        public decimal total_invoice { get; set; } = 0;
        public decimal total_items { get; set; } = 0;

        public OrderSellerView()
		{
		}
	}
}

