using Common.Integration;

namespace PaymentProvider.Services
{
    public interface IPaymentProvider
    {
        PaymentIntent ProcessPaymentIntent(PaymentIntentCreateOptions options);
    }
}

