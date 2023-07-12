using Common.Events;

namespace CustomerMS.Services
{
    public interface ICustomerService
    {
        void ProcessDeliveryNotification(DeliveryNotification paymentConfirmed);
        void ProcessPaymentConfirmed(PaymentConfirmed paymentConfirmed);
        void ProcessPaymentFailed(PaymentFailed paymentFailed);

        void Cleanup();
        void Reset();
    }
}

