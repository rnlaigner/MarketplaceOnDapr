using System;
using Common.Entities;
using Common.Events;
using Dapr.Client;

namespace CustomerMS.Services
{
	public class CustomerService : ICustomerService
	{
        public const string StoreName = "statestore";

        private readonly DaprClient daprClient;
        private readonly ILogger<CustomerService> logger;

        public CustomerService(DaprClient daprCLient, ILogger<CustomerService> logger)
		{
            this.daprClient = daprCLient;
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

        public void ProcessDeliveryNotification(DeliveryNotification paymentConfirmed)
        {
            throw new NotImplementedException();
        }

        public void ProcessPaymentConfirmed(PaymentConfirmed paymentConfirmed)
        {
            throw new NotImplementedException();
        }

        public void ProcessPaymentFailed(PaymentFailed paymentFailed)
        {
            throw new NotImplementedException();
        }
    }
}

