using System;
using System.ComponentModel.DataAnnotations.Schema;
using Common.Entities;
using Microsoft.EntityFrameworkCore;

namespace PaymentMS.Models
{

    [Table("order_payments")]
    [PrimaryKey(nameof(order_id), nameof(payment_sequential))]
    public class OrderPaymentModel
	{
        public long order_id { get; set; }

        // e.g., 1 - credit card, 2 - coupon, 3 - coupon
        public int payment_sequential { get; set; }

        // e.g., coupon, credit card
        public PaymentType payment_type { get; set; }

        // number of times the credit card is charged (usually once a month)
        public int payment_installments { get; set; }

        // respective to this line (ie. coupon)
        public decimal payment_value { get; set; }

        public OrderPaymentCardModel orderPaymentCard { get; set; }

        public OrderPaymentModel() { }

    }
}

