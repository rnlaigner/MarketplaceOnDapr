using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using PaymentMS.Infra;
using PaymentMS.Models;

namespace PaymentMS.Repositories;

public class PaymentRepository : IPaymentRepository
{
    private readonly PaymentDbContext dbContext;

    public PaymentRepository(PaymentDbContext paymentDbContext)
	{
        this.dbContext = paymentDbContext;
	}

    public OrderPaymentCardModel Insert(OrderPaymentCardModel orderPaymentCardModel)
    {
        return this.dbContext.OrderPaymentCards.Add(orderPaymentCardModel).Entity;
    }

    public OrderPaymentModel Insert(OrderPaymentModel orderPaymentModel)
    {
        return this.dbContext.OrderPayments.Add(orderPaymentModel).Entity;
    }

    public void InsertAll(List<OrderPaymentModel> paymentLines)
    {
        this.dbContext.OrderPayments.AddRange(paymentLines);
    }

    public IDbContextTransaction BeginTransaction()
    {
        return this.dbContext.Database.BeginTransaction();
    }

    public void Cleanup()
    {
        this.dbContext.OrderPaymentCards.ExecuteDelete();
        this.dbContext.OrderPayments.ExecuteDelete();
        this.dbContext.SaveChanges();
    }

    public void FlushUpdates()
    {
        this.dbContext.SaveChanges();
    }

    public IEnumerable<OrderPaymentModel> GetByOrderId(int customerId, int orderId)
    {
        return this.dbContext.OrderPayments.Where(o=> o.customer_id == customerId && o.order_id == orderId);
    }

}

