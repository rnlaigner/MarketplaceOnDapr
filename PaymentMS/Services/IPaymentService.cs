using Common.Events;

namespace PaymentMS.Services
{
	public interface IPaymentService
	{
        Task ProcessPayment(InvoiceIssued paymentRequest);
        void Cleanup();
        Task ProcessPoisonPayment(InvoiceIssued invoice);
    }
}

