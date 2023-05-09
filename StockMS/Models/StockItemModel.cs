using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Common.Entities;
using Common.Events;
using Microsoft.EntityFrameworkCore;

namespace StockMS.Models
{
    [Table("stock_items")]
    [PrimaryKey(nameof(product_id))]
    public class StockItemModel
	{

        public long product_id { get; set; }

        // consider partitioning on seller id
        // https://www.postgresql.org/docs/current/ddl-partitioning.html
        // https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/managing?tabs=dotnet-core-cli#adding-raw-sql
        public long seller_id { get; set; }

        public int qty_available { get; set; }

        public int qty_reserved { get; set; } = 0;

        public int order_count { get; set; } = 0;

        public int ytd { get; set; } = 0;

        public DateTime created_at { get; set; }
        public DateTime? updated_at { get; set; }

        // public bool active { get; set; } = true;

        public string? data { get; set; }

        private StockItemModel() { }

        public StockItemModel(long product_id, long seller_id, int qty, DateTime created_at)
		{
            this.product_id = product_id;
            this.seller_id = seller_id;
            this.qty_available = qty;
            this.created_at = created_at;
		}
	}
}

