using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using Common.Entities;

namespace ShipmentMS.Models
{
    [Table("packages", Schema = "shipment")]
    [PrimaryKey(nameof(order_id), nameof(package_id))]
    public class PackageModel
    {
        public int order_id { get; set; }

        public int package_id { get; set; }

        public int seller_id { get; set; }

        public int product_id { get; set; }

        public string product_name { get; set; } = "";

        public float freight_value { get; set; }

        public DateTime shipping_date { get; set; }

        public DateTime? delivery_date { get; set; }

        public int quantity { get; set; }

        public PackageStatus status { get; set; }

        public PackageModel() { }

    }

}

