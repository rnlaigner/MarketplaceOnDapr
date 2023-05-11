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

            // return an intent if already exists.
            // what about payment status success/failure?
            return new PaymentIntent()
            {
                    id = "",
                   amount = options.Amount,
                 // example: pi_1GszdL2eZvKYlo2C4nORvwio_secret_F06b3J3jgLq8Ueo5JeZUF79mr
                 client_secret = "",
                 currency = options.Currency.ToString(),
                 customer = options.Customer,
                 description = "",
                 created = DateTime.Now.Millisecond
            };
        }
    }
}

