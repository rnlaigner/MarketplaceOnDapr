using System;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using Common.Entities;

namespace SellerMS.Models
{

    [Table("order_entry_view")]
    [PrimaryKey(nameof(order_id))]
    [Index(nameof(seller_id))]
    public class OrderEntryViewModel
    {
        public long order_id { get; set; }

        public long seller_id { get; set; }

        public DateTime order_date { get; set; }

        public OrderStatus status { get; set; }

        // customer info
        public string customer_id { get; set; }

        public string first_name { get; set; }

        public string last_name { get; set; }

        public string street { get; set; }

        public string complement { get; set; }

        public string city { get; set; }

        public string state { get; set; }

        public string zip_code { get; set; }

        // shipment info (seller perspective)
        public int count_items { get; set; }

        public decimal total_amount { get; set; } = 0;
        public decimal total_freight { get; set; } = 0;
        public decimal total_incentive { get; set; } = 0;
        public decimal total_invoice { get; set; } = 0;
        public decimal total_items { get; set; } = 0;

        // payment info
        public string card_brand { get; set; }
        public int installments { get; set; }
        public int count_vouchers { get; set; }

        // payment. total used payment method. card, boleto, coupon, debit credit

        public OrderEntryViewModel()
		{
		}
	}
}

