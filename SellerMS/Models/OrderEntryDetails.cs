using System.ComponentModel.DataAnnotations.Schema;
using Common.Entities;
using Microsoft.EntityFrameworkCore;

namespace SellerMS.Models
{

    [Table("order_entry_details", Schema = "seller")]
    [PrimaryKey(nameof(order_id))]
    public class OrderEntryDetails
	{
        public int order_id { get; set; }

        public DateTime order_date { get; set; }

        public OrderStatus status { get; set; }

        // customer info
        public int customer_id { get; set; }

        public string first_name { get; set; } = "";

        public string last_name { get; set; } = "";

        public string street { get; set; } = "";

        public string complement { get; set; } = "";

        public string city { get; set; } = "";

        public string state { get; set; } = "";

        public string zip_code { get; set; } = "";

        // payment info
        public string card_brand { get; set; } = "";
        public int installments { get; set; }

        // public bool payment_success { get; set; } = true;

        // payment. total used payment method. card, boleto, coupon, debit credit

        public OrderEntryDetails()
		{
		}
	}
}

