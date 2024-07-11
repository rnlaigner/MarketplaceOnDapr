
using System.Collections.Concurrent;
using System.Collections.Generic;
using Common.Entities;
using Microsoft.EntityFrameworkCore.Storage;
using PaymentMS.Models;

namespace PaymentMS.Repositories;

public class InMemoryPaymentRepository : IPaymentRepository
{
    private readonly ConcurrentDictionary<(int customerId, int orderId, int packageId),OrderPaymentModel> orderPayments;
    private readonly ConcurrentDictionary<(int customerId, int orderId, int packageId),OrderPaymentCardModel> orderPaymentCards;

    private static readonly IDbContextTransaction DEFAULT_DB_TX = new NoTransactionScope();

	public InMemoryPaymentRepository()
	{
        this.orderPayments = new();
        this.orderPaymentCards = new();
	}

    public OrderPaymentCardModel Insert(OrderPaymentCardModel orderPaymentCard)
    {
        this.orderPaymentCards.TryAdd((orderPaymentCard.customer_id, orderPaymentCard.order_id, orderPaymentCard.sequential), orderPaymentCard);
        return orderPaymentCard;
    }

    public OrderPaymentModel Insert(OrderPaymentModel orderPayment)
    {
        this.orderPayments.TryAdd((orderPayment.customer_id, orderPayment.order_id, orderPayment.sequential), orderPayment);
        return orderPayment;
    }

    public IDbContextTransaction BeginTransaction()
    {
        return DEFAULT_DB_TX;
    }

    public void Cleanup()
    {
        this.orderPaymentCards.Clear();
        this.orderPayments.Clear();
    }

    public void FlushUpdates()
    {
        // do nothing
    }

    public IEnumerable<OrderPaymentModel> GetByOrderId(int customerId, int orderId)
    {
        return this.orderPayments.Values.Where(o=>o.customer_id == customerId && o.order_id == orderId);
    }

    public void InsertAll(List<OrderPaymentModel> paymentLines)
    {
        throw new NotImplementedException();
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

