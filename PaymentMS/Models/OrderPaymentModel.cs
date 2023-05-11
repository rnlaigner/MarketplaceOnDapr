using System;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace PaymentMS.Models
{

    [Table("order_payments")]
    [PrimaryKey(nameof(order_id))]
    public class OrderPaymentModel
	{
        public long order_id { get; set; }

        // 1 - coupon, 2 - coupon, 3 - credit card
        public int payment_sequential { get; set; }

        // coupon, credit card
        public string payment_type { get; set; }

        // number of times the credit card is charged (usually once a month)
        public int payment_installments { get; set; }

        // respective to this line (ie. coupon)
        public decimal payment_value { get; set; }
    }
}

