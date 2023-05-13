using System;
using Castle.DynamicProxy;

namespace ShipmentMS.Infra
{
    public class TransactionalInterceptor : IInterceptor
    {
        /* ideally should be using interface
        public TransactionalInterceptor(IDbContext dbContext)
        {
        }
        */

        private readonly ShipmentDbContext dbContext;

        private readonly ILogger<TransactionalInterceptor> logger;

        public TransactionalInterceptor(ShipmentDbContext dbContext, ILogger<TransactionalInterceptor> logger)
        {
            this.dbContext = dbContext;
            this.logger = logger;
        }

        public void Intercept(IInvocation invocation)
        {
            try
            {
                using (var txCtx = dbContext.Database.BeginTransaction())
                {
                    invocation.Proceed(); // Call the original method
                    this.dbContext.SaveChanges();
                    txCtx.Commit();
                }
            } catch(Exception e)
            {
                logger.LogError("[TransactionalInterceptor] {0}", e.Message);
            }
        }

    }

}

