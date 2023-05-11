using System;
using System.Data.Common;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace StockMS.Infra
{
    /*
	 * https://learn.microsoft.com/en-us/ef/core/logging-events-diagnostics/interceptors
	 */
    public class TransactionInterceptor : DbTransactionInterceptor
    {
		public TransactionInterceptor()
		{
		}

        public override DbTransaction TransactionStarted(DbConnection connection, TransactionEndEventData eventData, DbTransaction result)
		{

			// connection.BeginTransaction(System.Data.IsolationLevel.Serializable);

			var command = connection.CreateCommand();
			command.CommandText = "SET TRANSACTION ISOLATION LEVEL SERIALIZABLE;";
			command.ExecuteNonQuery();
			return result;
        }


    }
}

