﻿using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace CartMS.Models
{
    [Table("replica_products")]
    [PrimaryKey(nameof(seller_id), nameof(product_id))]
    public class ProductModel
    {
        public long seller_id { get; set; }

        public long product_id { get; set; }

        public string name { get; set; } = "";

        public string sku { get; set; } = "";

        public string category { get; set; } = "";

        public string description { get; set; } = "";

        public decimal price { get; set; }

        public decimal freight_value { get; set; }

        public DateTime created_at { get; set; }

        public DateTime updated_at { get; set; }

        public string status { get; set; } = "";

        public bool active { get; set; }

        public ProductModel()
        {
        }
    }
}

