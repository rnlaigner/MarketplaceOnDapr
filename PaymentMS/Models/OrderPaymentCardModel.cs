using System;
using System.ComponentModel.DataAnnotations.Schema;
using Common.Entities;
using Microsoft.EntityFrameworkCore;

namespace PaymentMS.Models
{

    [Table("order_payment_cards")]
    [PrimaryKey(nameof(order_id), nameof(payment_sequential))]
    public class OrderPaymentCardModel
	{

        [ForeignKey("OrderPayment")]
        public long order_id { get; set; }
        public OrderPaymentModel OrderPayment { get; set; }

        public int payment_sequential { get; set; }

        // card info coming from customer checkout
        public string card_number { get; set; }

        public string card_holder_name { get; set; }

        public DateTime card_expiration { get; set; }

        // public string card_security_number { get; set; }

        public string card_brand { get; set; }

        public OrderPaymentCardModel()
		{
		}
	}
}

