using System;
using Common.Events;

namespace PaymentMS.Services
{
	public interface IPaymentService
	{
        bool ProcessPayment(PaymentRequest paymentRequest);

        Task<bool> ProcessPaymentAsync(PaymentRequest paymentRequest);
    }
}

