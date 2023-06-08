using System;
using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace ShipmentMS.Infra
{
    public class TransactionInterceptor : DbTransactionInterceptor
    {
		public TransactionInterceptor()
		{
		}

        public override InterceptionResult<DbTransaction> TransactionStarting(
            DbConnection connection, TransactionStartingEventData eventData, InterceptionResult<DbTransaction> result)
        {
            // "This value will have HasResult set to true if some previous
            // interceptor suppressed execution by calling SuppressWithResult"
            if (result.HasResult)
            {
                // Use the existing transaction
                return result;
            }
            
            var transaction = connection.BeginTransaction(System.Data.IsolationLevel.Serializable);
            return InterceptionResult<DbTransaction>.SuppressWithResult(transaction);

        }

    }
}

