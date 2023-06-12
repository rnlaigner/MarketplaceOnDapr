using System;
using PaymentMS.Models;

namespace PaymentMS.Repositories
{
	public interface IPaymentRepository
	{

        IEnumerable<OrderPaymentModel> GetByOrderId(long orderId);

    }
}

