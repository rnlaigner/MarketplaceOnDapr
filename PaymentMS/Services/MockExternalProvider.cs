using System;
using Common.Entities;
using PaymentMS.Integration;

namespace PaymentMS.Services
{
	public class MockExternalProvider : IExternalProvider
	{
        private readonly int delay;

		public MockExternalProvider(int delay = 1000)
		{
            this.delay = delay;
		}

        public async Task<PaymentIntent> Create(PaymentIntentCreateOptions options)
        {
            await Task.Delay(delay);

            // TODO perform http request to driver
            return new PaymentIntent()
            {
                  Id = Guid.NewGuid().ToString(),
                  Amount = options.Amount,
                 // example: pi_1GszdL2eZvKYlo2C4nORvwio_secret_F06b3J3jgLq8Ueo5JeZUF79mr
                 client_secret = "",
                 Currency = options.Currency.ToString(),
                 Customer = options.Customer,
                 FailureMessage = "",
                 Created = DateTime.Now.Millisecond
            };
        }
    }
}

