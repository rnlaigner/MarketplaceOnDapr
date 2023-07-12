using Common.Events;
using CustomerMS.Infra;
using CustomerMS.Repositories;
using Microsoft.EntityFrameworkCore;

namespace CustomerMS.Services
{
	public class CustomerService : ICustomerService
	{
        private readonly ICustomerRepository customerRepository;
        private readonly CustomerDbContext dbContext;
        private readonly ILogger<CustomerService> logger;

        public CustomerService(ICustomerRepository customerRepository, CustomerDbContext dbContext, ILogger<CustomerService> logger)
		{
            this.customerRepository = customerRepository;
            this.dbContext = dbContext;
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
            this.dbContext.Customers.ExecuteDelete();
            this.dbContext.SaveChanges();
        }

        public void Reset()
        {
            this.dbContext.Database.ExecuteSqlRaw("UPDATE customers SET delivery_count=0, failed_payment_count=0, success_payment_count=0");
            this.dbContext.SaveChanges();
        }

    }
}

