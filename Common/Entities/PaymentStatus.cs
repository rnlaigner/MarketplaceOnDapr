using System;
namespace Common.Entities
{
    /*
     * Based on: https://stripe.com/docs/payments/payment-intents/verifying-status
     */
    public enum PaymentStatus
	{
        succeeded,
        payment_failed
    }
}

