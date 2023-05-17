using System;
namespace Common.Events
{
	public class ProductPriceUpdate
	{

		public long productId { get; set; }
        public string sku { get; set; }
        public decimal newPrice { get; set; }
        public decimal oldPrice { get; set; }
        public string instanceId { get; set; }

        public ProductPriceUpdate()
		{
		}
	}
}

