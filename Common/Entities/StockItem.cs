using System;
namespace Common.Entities
{
	public class StockItem
	{
        public long seller_id { get; set; }

        public long product_id { get; set; }

        public int qty_available { get; set; }

        public int qty_reserved { get; set; }

        public int order_count { get; set; }

        public int ytd { get; set; }

        public string? data { get; set; }

    }
}