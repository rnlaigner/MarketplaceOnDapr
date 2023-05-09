using System;
using Common.Entities;

namespace Common.Events
{
    public record Checkout
    {
        public readonly DateTime createdAt;
        public readonly CustomerCheckout customerCheckout;
        public readonly IDictionary<long, CartItem> items;
        public readonly string instanceId;

        public Checkout(DateTime createdAt, CustomerCheckout customerCheckout, IDictionary<long, CartItem> items, string instanceId = "")
        {
            this.createdAt = createdAt;
            this.customerCheckout = customerCheckout;
            this.items = items;
            this.instanceId = instanceId;
        }

    }
}

