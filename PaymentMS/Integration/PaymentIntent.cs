using System;
namespace PaymentMS.Integration
{
	/**
     * Inspired by Stripe payment system
	 * https://stripe.com/docs/api/payment_intents
	 * https://stripe.com/docs/payments/payment-intents
	 * 
	 * https://stripe.com/docs/payments/quickstart?lang=dotnet&client=java&platform=android
	 * "A PaymentIntent tracks the customer’s payment lifecycle{ get; set; } 
	 * keeping track of any failed payment attempts and ensuring 
	 * the customer is only charged once. Return the PaymentIntent’s 
	 * client secret in the response to finish the payment on the client."
	 * 
	 * "We recommend that you create exactly one PaymentIntent for each order 
	 * or customer session in your system. You can reference the PaymentIntent 
	 * later to see the history of payment attempts for a particular session."
	 */
	public class PaymentIntent
	{
		// example: pi_1GszdL2eZvKYlo2C4nORvwio
		public string Id { get; set; } = "";
        public decimal Amount { get; set; }
		public string Status { get; set; } = "";// 'succeeded', requires_payment_method
        // example: pi_1GszdL2eZvKYlo2C4nORvwio_secret_F06b3J3jgLq8Ueo5JeZUF79mr
        public string client_secret { get; set; } = "";
        public string Currency { get; set; } = "";
        public string Customer { get; set; } = "";
        public string FailureMessage { get; set; } = "";
        public long Created { get; set; }

	}
}

