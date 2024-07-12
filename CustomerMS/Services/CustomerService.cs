using Common.Events;
using CustomerMS.Infra;
using CustomerMS.Repositories;

namespace CustomerMS.Services;

public class CustomerService : ICustomerService
{
    private readonly ICustomerRepository customerRepository;
    private readonly ILogger<CustomerService> logger;

    public CustomerService(ICustomerRepository customerRepository, ILogger<CustomerService> logger)
    {
        this.customerRepository = customerRepository;
        this.logger = logger;
    }

    public void ProcessDeliveryNotification(DeliveryNotification deliveryNotification)
    {
        var customer = this.customerRepository.GetById(deliveryNotification.customerId);
        if(customer is not null)
        {
            customer.delivery_count++;
            this.customerRepository.Update(customer);
        }
    }

    public void ProcessPaymentConfirmed(PaymentConfirmed paymentConfirmed)
    {
        var customer = this.customerRepository.GetById(paymentConfirmed.customer.CustomerId);
        if (customer is not null)
        {
            customer.success_payment_count++;
            this.customerRepository.Update(customer);
        }
    }

    public void ProcessPaymentFailed(PaymentFailed paymentFailed)
    {
        var customer = this.customerRepository.GetById(paymentFailed.customer.CustomerId);
        if (customer is not null)
        {
            customer.failed_payment_count++;
            this.customerRepository.Update(customer);
        }
    }

    public void Cleanup()
    {
        this.customerRepository.Cleanup();
    }

    public void Reset()
    {
        this.customerRepository.Reset();
    }

}


