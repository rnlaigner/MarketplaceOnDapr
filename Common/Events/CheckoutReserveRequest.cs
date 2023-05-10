using System;
using Common.Entities;

namespace Common.Events
{
    public record CheckoutReserveRequest
    {
        public readonly DateTime createdAt;
        public readonly CustomerCheckout customerCheckout;
        public readonly List<CartItem> items;
        public readonly string instanceId;

        public CheckoutReserveRequest(DateTime createdAt, CustomerCheckout customerCheckout, List<CartItem> items, string instanceId = "")
        {
            this.createdAt = createdAt;
            this.customerCheckout = customerCheckout;
            this.items = items;
            this.instanceId = instanceId;
        }

    }
}

