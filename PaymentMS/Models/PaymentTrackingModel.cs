using System;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace PaymentMS.Models
{

    [Table("payment_tracking")]
    [PrimaryKey(nameof(instanceId))]
    public class PaymentTrackingModel
	{

        public string instanceId { get; set; }
        public string status { get; set; }

        public PaymentTrackingModel()
		{
		}
	}
}

