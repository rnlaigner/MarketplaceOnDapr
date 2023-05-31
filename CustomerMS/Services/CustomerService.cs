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

        public async void ProcessDeliveryNotification(DeliveryNotification deliveryNotification)
        {
            Customer customer = await daprClient.GetStateAsync<Customer>(StoreName, deliveryNotification.customerId);
            customer.delivery_count++;
            await daprClient.SaveStateAsync(StoreName, deliveryNotification.customerId, customer);
        }

        public async void ProcessPaymentConfirmed(PaymentConfirmed paymentConfirmed)
        {
            Customer customer = await daprClient.GetStateAsync<Customer>(StoreName, paymentConfirmed.customer.CustomerId);
            customer.success_payment_count++;
            await daprClient.SaveStateAsync(StoreName, paymentConfirmed.customer.CustomerId, customer);
        }

        public async void ProcessPaymentFailed(PaymentFailed paymentFailed)
        {
            Customer customer = await daprClient.GetStateAsync<Customer>(StoreName, paymentFailed.customer.CustomerId);
            customer.failed_payment_count++;
            await daprClient.SaveStateAsync(StoreName, paymentFailed.customer.CustomerId, customer);
        }

    }
}

