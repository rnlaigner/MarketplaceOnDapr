using System;
using System.Collections.Generic;
using OrderMS.Common.Models;
using OrderMS.Common.Repositories;
using OrderMS.Infra;

namespace OrderMS.Repositories
{
    /*
     * https://learn.microsoft.com/en-us/ef/core/logging-events-diagnostics/interceptors
     * https://stackoverflow.com/questions/57441301/transactional-annotation-attribute-in-net-core
     * https://stackoverflow.com/questions/51783365/logging-using-aop-in-net-core-2-1
     * https://learn.microsoft.com/en-us/ef/core/logging-events-diagnostics/interceptors
     */
    public class OrderRepository : IOrderRepository
    {

        private readonly OrderDbContext dbContext;

        public OrderRepository(OrderDbContext orderingContext)
        {
            this.dbContext = orderingContext ?? throw new ArgumentNullException(nameof(orderingContext));
        }

        public IEnumerable<OrderModel> GetAll()
        {
            return this.dbContext.Orders;
        }

    }

}