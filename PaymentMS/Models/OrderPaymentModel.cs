using System.ComponentModel.DataAnnotations.Schema;
using Common.Entities;
using Common.Integration;
using Microsoft.EntityFrameworkCore;

namespace PaymentMS.Models
{

    [Table("order_payments", Schema = "payment")]
    [PrimaryKey(nameof(order_id), nameof(sequential))]
    public class OrderPaymentModel
	{
        public int order_id { get; set; }

        // e.g., 1 - credit card, 2 - coupon, 3 - coupon
        public int sequential { get; set; }

        // e.g., coupon, credit card
        public PaymentType type { get; set; }

        // number of times the credit card is charged (usually once a month)
        public int installments { get; set; }

        // respective to this line (ie. coupon)
        public float value { get; set; }

        public DateTime created_at { get; set; }

        // vouchers dont need to have this field filled
        public PaymentStatus? status { get; set; }

        public OrderPaymentCardModel? orderPaymentCard { get; set; }

        public OrderPaymentModel() { }

    }
}

