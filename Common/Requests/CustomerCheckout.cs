namespace Common.Requests
{
    /**
     * A sub-type of customer.
     * Ideally; address and credit card info may change across customer checkouts
     * Basket and Order does not need to know all internal data about customers
     */
    public class CustomerCheckout {
    
        public int CustomerId;

        /**
        * Delivery address (could be different from customer's address)
        */
        public string FirstName;

        public string LastName;

        public string Street;

        public string Complement;

        public string City;

        public string State;

        public string ZipCode;

        /**
        * Payment type
        */
        public string PaymentType;

        /**
        * Credit or debit card
        */
        public string CardNumber;

        public string CardHolderName;

        public string CardExpiration;

        public string CardSecurityNumber;

        public string CardBrand;

        // if no credit card; must be 1
        public int Installments;

        public string instanceId;

        public CustomerCheckout(){ }

    }
    
}