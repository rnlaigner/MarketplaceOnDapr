using System;
using Common.Entities;
using Common.Events;
using CustomerMS.Repositories;
using Dapr.Client;

namespace CustomerMS.Services
{
	public class CustomerService : ICustomerService
	{
        public const string StoreName = "statestore";

        private readonly DaprClient daprClient;
        private readonly ICustomerRepository customerRepository;
        private readonly ILogger<CustomerService> logger;

        public CustomerService(DaprClient daprCLient, ICustomerRepository customerRepository, ILogger<CustomerService> logger)
		{
            this.daprClient = daprCLient;
            this.customerRepository = customerRepository;
            this.logger = logger;
		}

        public async void AddCustomer(Customer customer)
        {
            await daprClient.SaveStateAsync(StoreName, customer.id.ToString(), customer);
        }

        public async Task<Customer> GetCustomer(long id)
        {
            return await daprClient.GetStateAsync<Customer>(StoreName, id.ToString());
        }

        public async void ProcessDeliveryNotification(DeliveryNotification deliveryNotification)
        {
            await this.customerRepository.IncreaseDeliveryAtomically(deliveryNotification.customerId);
        }

        public async void ProcessPaymentConfirmed(PaymentConfirmed paymentConfirmed)
        {
            await this.customerRepository.IncreasePaymentSuccessCount(paymentConfirmed);
        }

        public async void ProcessPaymentFailed(PaymentFailed paymentFailed)
        {
            await this.customerRepository.IncreasePaymentFailureCount(paymentFailed);
        }

    }
}

