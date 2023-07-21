using System.Data.Common;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace OrderMS.Infra
{
    public class TransactionInterceptor : DbTransactionInterceptor
    {
		public TransactionInterceptor()
		{
		}

        public override InterceptionResult<DbTransaction> TransactionStarting(
            DbConnection connection, TransactionStartingEventData eventData, InterceptionResult<DbTransaction> result)
        {
            if (result.HasResult)
            {
                return result;
            }
            
            var transaction = connection.BeginTransaction(System.Data.IsolationLevel.Serializable);
            return InterceptionResult<DbTransaction>.SuppressWithResult(transaction);

        }

    }
}

