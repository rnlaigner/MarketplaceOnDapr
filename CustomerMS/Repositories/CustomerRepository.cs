using System;
using Common.Entities;
using Common.Events;
using System.Security.Cryptography.X509Certificates;
using Dapr.Client;

namespace CustomerMS.Repositories
{
	public class CustomerRepository : ICustomerRepository
    {
        public const string StoreName = "statestore";
        private readonly DaprClient daprClient;

        public CustomerRepository(DaprClient daprCLient)
		{
            this.daprClient = daprCLient;
        }

        public async Task IncreaseDeliveryAtomically(long customerId)
        {
            var custIdStr = customerId.ToString();
            while (true)
            {
                var entry = await daprClient.GetStateEntryAsync<Customer>(StoreName, custIdStr);
                Customer customer = entry.Value;
                customer.delivery_count++;

                if (await daprClient.TrySaveStateAsync(StoreName, custIdStr, customer, entry.ETag))
                {
                    break;
                }

            }
        }

        public async Task IncreasePaymentSuccessCount(PaymentConfirmed paymentConfirmed)
        {
            var custIdStr = paymentConfirmed.customer.CustomerId.ToString();
            while (true)
            {
                var entry = await daprClient.GetStateEntryAsync<Customer>(StoreName, custIdStr);
                Customer customer = entry.Value;
                customer.success_payment_count++;

                if (await daprClient.TrySaveStateAsync(StoreName, custIdStr, customer, entry.ETag))
                {
                    break;
                }

            }
        }

        public async Task IncreasePaymentFailureCount(PaymentFailed paymentFailed)
        {
            var custIdStr = paymentFailed.customer.CustomerId.ToString();
            while (true)
            {
                var entry = await daprClient.GetStateEntryAsync<Customer>(StoreName, custIdStr);
                Customer customer = entry.Value;
                customer.failed_payment_count++;

                if (await daprClient.TrySaveStateAsync(StoreName, custIdStr, customer, entry.ETag))
                {
                    break;
                }

            }
        }

    }
}

