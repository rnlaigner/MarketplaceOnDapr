using System;
using Common.Events;

namespace CustomerMS.Repositories
{
	public interface ICustomerRepository
	{
		Task IncreaseDeliveryAtomically(long customerId);
        Task IncreasePaymentSuccessCount(PaymentConfirmed paymentConfirmed);
        Task IncreasePaymentFailureCount(PaymentFailed paymentFailed);
    }
}

