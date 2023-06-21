namespace Common.Events
{
    public class ProductUpdate
	{
        public long seller_id { get; set; }

        public long product_id { get; set; }

        public decimal price { get; set; }

        public bool active { get; set; }

        public int instanceId { get; set; }

        public ProductUpdate() { }

    }
}