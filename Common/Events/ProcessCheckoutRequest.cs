using System;
using Common.Entities;

namespace Common.Events
{
    public record ProcessCheckoutRequest
    {
        public readonly DateTime createdAt;
        public readonly CustomerCheckout customerCheckout;
        public readonly List<CartItem> items;
        public readonly string instanceId;

        public ProcessCheckoutRequest(DateTime createdAt, CustomerCheckout customerCheckout, List<CartItem> items, string instanceId = "")
        {
            this.createdAt = createdAt;
            this.customerCheckout = customerCheckout;
            this.items = items;
            this.instanceId = instanceId;
        }

    }
}

