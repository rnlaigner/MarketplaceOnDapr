using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Storage;
using OrderMS.Common.Models;
using OrderMS.Common.Repositories;

namespace OrderMS.Repositories;

public class InMemoryOrderRepository : IOrderRepository
{
    private readonly ConcurrentDictionary<(int customerId, int orderId),OrderModel> orders;
    private readonly ConcurrentDictionary<int,CustomerOrderModel> customerOrders;

    private readonly ConcurrentDictionary<(int customerId, int orderId),List<OrderItemModel>> orderItems;
    private readonly ConcurrentDictionary<(int customerId, int orderId),List<OrderHistoryModel>> orderHistory;

    private static readonly IDbContextTransaction DEFAULT_DB_TX = new NoTransactionScope();

	public InMemoryOrderRepository()
	{
        this.orders = new();
        this.customerOrders = new();
        this.orderItems = new();
        this.orderHistory = new();
	}

    public IEnumerable<OrderModel> GetAll()
    {
        return this.orders.Values;
    }

    public IEnumerable<OrderModel> GetByCustomerId(int customerId)
    {
        return this.orders.Values.Where(o=> o.customer_id == customerId);
    }

    public OrderModel? GetOrder(int customerId, int orderId)
    {
        return this.orders[(customerId, orderId)];
    }

    public OrderModel InsertOrder(OrderModel order)
    {
        this.orders.TryAdd((order.customer_id,order.order_id), order);
        return order;
    }

    public OrderModel UpdateOrder(OrderModel order)
    {
        this.orders[(order.customer_id,order.order_id)] = order;
        return order;
    }

    public IDbContextTransaction BeginTransaction()
    {
        return DEFAULT_DB_TX;
    }

    public CustomerOrderModel? GetCustomerOrderByCustomerId(int customerId)
    {
        if(this.customerOrders.ContainsKey(customerId))
            return this.customerOrders[customerId];
        return null;
    }

    public CustomerOrderModel InsertCustomerOrder(CustomerOrderModel customerOrder)
    {
        this.customerOrders.TryAdd(customerOrder.customer_id, customerOrder);
        return customerOrder;
    }

    public CustomerOrderModel UpdateCustomerOrder(CustomerOrderModel customerOrder)
    {
        this.customerOrders[customerOrder.customer_id] = customerOrder;
        return customerOrder;
    }

    public OrderItemModel InsertOrderItem(OrderItemModel orderItem)
    {
        if(!this.orderItems.ContainsKey((orderItem.customer_id, orderItem.order_id))){
            this.orderItems[(orderItem.customer_id, orderItem.order_id)] = new();
        }
        this.orderItems[(orderItem.customer_id, orderItem.order_id)].Add(orderItem);
        return orderItem;
    }

    public OrderHistoryModel InsertOrderHistory(OrderHistoryModel orderHistory)
    {
        if(!this.orderHistory.ContainsKey((orderHistory.customer_id, orderHistory.order_id))){
            this.orderHistory[(orderHistory.customer_id, orderHistory.order_id)] = new();
        }
        this.orderHistory[(orderHistory.customer_id, orderHistory.order_id)].Add(orderHistory);
        return orderHistory;
    }

    public void FlushUpdates()
    {
        // do nothing
    }

    public void Cleanup()
    {
        this.orderHistory.Clear();
        this.orderItems.Clear();
        this.orders.Clear();
        this.customerOrders.Clear();
    }

    public class NoTransactionScope : IDbContextTransaction
    {
        public Guid TransactionId => throw new NotImplementedException();

        public void Commit()
        {
            // do nothing
        }

        public Task CommitAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            // do nothing
        }

        public ValueTask DisposeAsync()
        {
            throw new NotImplementedException();
        }

        public void Rollback()
        {
            throw new NotImplementedException();
        }

        public Task RollbackAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}


