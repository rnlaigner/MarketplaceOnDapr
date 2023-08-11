using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProductMS.Models
{
    [Table("products", Schema = "product")]
    [PrimaryKey(nameof(seller_id),nameof(product_id))]
    public class ProductModel
	{
        public int seller_id { get; set; }

        public int product_id { get; set; }

        public string name { get; set; } = "";

        public string sku { get; set; } = "";

        public string category { get; set; } = "";

        public string description { get; set; } = "";

        public float price { get; set; }

        public float freight_value { get; set; }

        public DateTime created_at { get; set; }

        public DateTime updated_at { get; set; }

        public string status { get; set; } = "approved";

        public int version { get; set; }

        public bool active { get; set; }

        public ProductModel()
		{
		}
	}
}

