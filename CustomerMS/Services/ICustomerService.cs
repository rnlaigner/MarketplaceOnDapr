using System;
using Common.Entities;
using Common.Events;

namespace CustomerMS.Services
{
    public interface ICustomerService
    {
        void AddCustomer(Customer customer);
        Task<Customer> GetCustomer(long id);

        void ProcessDeliveryNotification(DeliveryNotification paymentConfirmed);
        void ProcessPaymentConfirmed(PaymentConfirmed paymentConfirmed);
        void ProcessPaymentFailed(PaymentFailed paymentFailed);
    }
}

