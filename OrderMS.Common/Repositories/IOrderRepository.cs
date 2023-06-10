using System;
using OrderMS.Common.Models;

namespace OrderMS.Common.Repositories
{
	public interface IOrderRepository
	{
        public IEnumerable<OrderModel> GetAll();
        public IEnumerable<OrderModel> GetByCustomerId(long customerId);
        public OrderModel? GetOrder(long orderId);
    }
}

