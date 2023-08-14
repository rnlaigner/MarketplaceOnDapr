using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace CartMS.Models
{
    [Table("replica_products", Schema = "cart")]
    [PrimaryKey(nameof(seller_id), nameof(product_id))]
    public class ProductModel
    {
        public int seller_id { get; set; }

        public int product_id { get; set; }

        public string name { get; set; }

        public float price { get; set; }

        public int version { get; set; }

        public bool active { get; set; }

        public DateTime created_at { get; set; }

        public DateTime updated_at { get; set; }

        public ProductModel()
        {
            this.active = true;
        }
    }
}

