using System;
using PaymentMS.Integration;

namespace PaymentMS.Services
{
    /**
     * 
     * Based on
     * https://stripe.com/docs/payments/quickstart?lang=dotnet&client=java&platform=android
     * 
     */
    public interface IExternalProvider
	{
        PaymentIntent Create(PaymentIntentCreateOptions options);

    }
}

