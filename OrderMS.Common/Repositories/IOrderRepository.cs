using Microsoft.EntityFrameworkCore.Storage;
using OrderMS.Common.Models;

namespace OrderMS.Common.Repositories
{
	public interface IOrderRepository
	{
        public IEnumerable<OrderModel> GetAll();
        public IEnumerable<OrderModel> GetByCustomerId(int customerId);
        public OrderModel? GetOrder(int customerId, int orderId);
        public OrderModel InsertOrder(OrderModel order);
        public OrderModel UpdateOrder(OrderModel order);

        // APIs for OrderService
        public IDbContextTransaction BeginTransaction();
        public CustomerOrderModel? GetCustomerOrderByCustomerId(int customerId);
        public CustomerOrderModel InsertCustomerOrder(CustomerOrderModel customerOrder);
        public CustomerOrderModel UpdateCustomerOrder(CustomerOrderModel customerOrder);
        public OrderItemModel InsertOrderItem(OrderItemModel orderItem);
        public OrderHistoryModel InsertOrderHistory(OrderHistoryModel orderHistory);
        public void FlushUpdates();
        public void Cleanup();

    }
}

