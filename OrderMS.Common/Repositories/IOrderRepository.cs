using System;
using OrderMS.Common.Models;

namespace OrderMS.Common.Repositories
{
	public interface IOrderRepository
	{
        public IEnumerable<OrderModel> GetAll();
        public IEnumerable<OrderModel> GetByCustomerId(int customerId);
        public OrderModel? GetOrder(int orderId);
    }
}

