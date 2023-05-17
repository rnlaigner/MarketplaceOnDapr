using System;
using Common.Events;

namespace PaymentMS.Services
{
	public interface IPaymentService
	{
        Task ProcessPayment(ProcessPayment paymentRequest);
    }
}

