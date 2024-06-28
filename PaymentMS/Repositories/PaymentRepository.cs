using PaymentMS.Infra;
using PaymentMS.Models;

namespace PaymentMS.Repositories
{
	public class PaymentRepository : IPaymentRepository
	{
        private readonly PaymentDbContext dbContext;

        public PaymentRepository(PaymentDbContext paymentDbContext)
		{
            this.dbContext = paymentDbContext;
		}

        public IEnumerable<OrderPaymentModel> GetByOrderId(int orderId)
        {
            return this.dbContext.OrderPayments.Where(o=>o.order_id == orderId);
        }
    }
}

