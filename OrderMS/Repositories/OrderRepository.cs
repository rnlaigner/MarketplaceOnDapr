using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using OrderMS.Common.Models;
using OrderMS.Common.Repositories;
using OrderMS.Infra;

namespace OrderMS.Repositories;

/*
 * https://learn.microsoft.com/en-us/ef/core/logging-events-diagnostics/interceptors
 * https://stackoverflow.com/questions/57441301/transactional-annotation-attribute-in-net-core
 * https://stackoverflow.com/questions/51783365/logging-using-aop-in-net-core-2-1
 * https://learn.microsoft.com/en-us/ef/core/logging-events-diagnostics/interceptors
 */
public class OrderRepository : IOrderRepository
{

    private readonly OrderDbContext dbContext;

    public OrderRepository(OrderDbContext dbContext)
    {
        this.dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public IEnumerable<OrderModel> GetAll()
    {
        return this.dbContext.Orders;
    }

    public IEnumerable<OrderModel> GetByCustomerId(int customerId)
    {
        return this.dbContext.Orders.Where(o => o.customer_id == customerId);
    }

    public OrderModel? GetOrder(int customerId, int orderId)
    {
        return this.dbContext.Orders.Find(customerId, orderId);
    }
    
    public IDbContextTransaction BeginTransaction()
    {
        return this.dbContext.Database.BeginTransaction();
    }

    public OrderModel InsertOrder(OrderModel order)
    {
        return this.dbContext.Orders.Add(order).Entity;
    }

    public OrderModel UpdateOrder(OrderModel order)
    {
        return this.dbContext.Orders.Update(order).Entity;
    }

    // TODO this must be for update to lock the row
    public CustomerOrderModel? GetCustomerOrderByCustomerId(int customerId)
    {
        return this.dbContext.CustomerOrders.Find(customerId);
    }

    public CustomerOrderModel InsertCustomerOrder(CustomerOrderModel customerOrder)
    {
        return this.dbContext.CustomerOrders.Add(customerOrder).Entity;
    }

    public CustomerOrderModel UpdateCustomerOrder(CustomerOrderModel customerOrder)
    {
        return this.dbContext.CustomerOrders.Update(customerOrder).Entity;
    }

    public OrderItemModel InsertOrderItem(OrderItemModel orderItem)
    {
        return this.dbContext.OrderItems.Add(orderItem).Entity;
    }

    public OrderHistoryModel InsertOrderHistory(OrderHistoryModel orderHistory)
    {
        return this.dbContext.OrderHistory.Add(orderHistory).Entity;
    }

    public void FlushUpdates()
    {
        this.dbContext.SaveChanges();
    }

    public void Cleanup()
    {
        this.dbContext.OrderItems.ExecuteDelete();
        this.dbContext.OrderHistory.ExecuteDelete();
        this.dbContext.Orders.ExecuteDelete();
     
        this.dbContext.CustomerOrders.ExecuteDelete();
        this.dbContext.SaveChanges();
    }
}

