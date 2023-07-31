namespace SellerMS.Models
{
    /*
    [Table("product_entries")]
    [PrimaryKey(nameof(product_id))]
    [Index(nameof(seller_id))]
    */
    public class ProductEntry
	{
        public int seller_id { get; set; }

        // product item
        public int product_id { get; set; }
        public string sku { get; set; } = "";
        public string category { get; set; } = "";
        public string description { get; set; } = "";
        public float price { get; set; }

        // stock item
        public int qty_available { get; set; }
        public int qty_reserved { get; set; } = 0;

        // how many orders involved
        public int order_count { get; set; } = 0;

        // how many units sold
        public int item_count { get; set; } = 0;

        public float total_revenue { get; set; } = 0;
        public float total_discount { get; set; } = 0;

        public ProductEntry()
		{
		}
	}
}

