using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using Common.Entities;

namespace ShipmentMS.Models
{
    [Table("shipments", Schema = "shipment")]
    [PrimaryKey(nameof(order_id))]
    public class ShipmentModel
	{
        public int order_id { get; set; }
        public int customer_id { get; set; }

        public int package_count { get; set; }
        public float total_freight_value { get; set; }

        public DateTime request_date { get; set; }

        public ShipmentStatus status { get; set; }

        // customer
        public string first_name { get; set; } = "";

        public string last_name { get; set; } = "";

        public string street { get; set; } = "";

        public string complement { get; set; } = "";

        public string zip_code { get; set; } = "";

        public string city { get; set; } = "";

        public string state { get; set; } = "";

        [ForeignKey("order_id")]
        public ICollection<PackageModel> packages { get; } = new List<PackageModel>();

        public ShipmentModel()
		{
		}

	}
}

