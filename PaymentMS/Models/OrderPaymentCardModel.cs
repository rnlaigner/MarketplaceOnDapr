using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace PaymentMS.Models
{

    [Table("order_payment_cards", Schema = "payment")]
    [PrimaryKey(nameof(order_id), nameof(payment_sequential))]
    public class OrderPaymentCardModel
	{

        public int order_id { get; set; }

        public int payment_sequential { get; set; }

        public OrderPaymentModel orderPayment { get; set; }

        // card info coming from customer checkout
        public string card_number { get; set; } = "";

        public string card_holder_name { get; set; } = "";

        public DateTime card_expiration { get; set; }

        // for privacy issues, we assume this is not stored, only carried out in the event
        // public string card_security_number { get; set; }

        public string card_brand { get; set; } = "";

        public OrderPaymentCardModel()
		{
		}
	}
}

