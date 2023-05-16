using System;
using System.Data;
using System.Reflection;
using Castle.DynamicProxy;
using Microsoft.EntityFrameworkCore;

namespace ShipmentMS.Infra
{
    public class TransactionalInterceptor : IInterceptor
    {
        /* ideally should be using interface
        public TransactionalInterceptor(IDbContext dbContext)
        {
        }
        */

        private readonly IsolationLevel defaultIsolationlevel;

        private readonly ShipmentDbContext dbContext;

        private readonly ILogger<TransactionalInterceptor> logger;

        public TransactionalInterceptor(ShipmentDbContext dbContext, ILogger<TransactionalInterceptor> logger, System.Data.IsolationLevel defaultIsolationlevel = IsolationLevel.ReadCommitted)
        {
            this.dbContext = dbContext;
            this.logger = logger;
            this.defaultIsolationlevel = defaultIsolationlevel;
        }

        public void Intercept(IInvocation invocation)
        {

            // Get the TransactionalAttribute of the method
            var transactionalAttribute = invocation.Method.GetCustomAttribute<TransactionalAttribute>();

            // set isolation explicitly. do not rely on default isolation level of postgresSQL
            var definedIsolationlevel = transactionalAttribute != null ? transactionalAttribute.IsolationLevel : defaultIsolationlevel;
     
            using (var txCtx = dbContext.Database.GetDbConnection().BeginTransaction(definedIsolationlevel))
            {

                try
                {
                    invocation.Proceed(); // Call the original method
                    this.dbContext.SaveChanges();
                    txCtx.Commit();
                } catch(Exception e)
                {
                    this.logger.LogError("[TransactionalInterceptor] {0}. Transaction {1} will be rollbacked.", e.Message, txCtx.GetHashCode());
                    // ?
                    txCtx.Rollback();
                    
                }
            }

        }

    }

}

