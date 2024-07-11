using Microsoft.EntityFrameworkCore.Storage;
using PaymentMS.Models;

namespace PaymentMS.Repositories;

public interface IPaymentRepository
{
    IEnumerable<OrderPaymentModel> GetByOrderId(int customerId, int orderId);

    OrderPaymentCardModel Insert(OrderPaymentCardModel orderPaymentCard);
    OrderPaymentModel Insert(OrderPaymentModel orderPayment);

    // APIs for PaymentService
    IDbContextTransaction BeginTransaction();
    void FlushUpdates();
    void Cleanup();
    void InsertAll(List<OrderPaymentModel> paymentLines);
}
