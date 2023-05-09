using System;
using OrderMS.Test.Infra;
using OrderMS.Repositories;
using OrderMS.Common.Models;

namespace OrderMS.Test.Repositories
{
    /*
     * https://learn.microsoft.com/en-us/ef/core/logging-events-diagnostics/interceptors
     * https://stackoverflow.com/questions/57441301/transactional-annotation-attribute-in-net-core
     * https://stackoverflow.com/questions/51783365/logging-using-aop-in-net-core-2-1
     */
    public class SQLiteOrderRepository : IOrderRepository
    {

        private readonly OrderDbContext dbContext;

        public SQLiteOrderRepository(OrderDbContext orderingContext)
        {
            this.dbContext = orderingContext ?? throw new ArgumentNullException(nameof(orderingContext));
        }

        public IEnumerable<OrderModel> GetAll()
        {
            return this.dbContext.Orders;
        }

    }

}