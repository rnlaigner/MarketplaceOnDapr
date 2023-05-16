using System;
using Common.Entities;

namespace Common.Events
{
    public record ReserveStockFailed
    {
        public readonly DateTime createdAt;
        public readonly CustomerCheckout customerCheckout;
        public readonly string instanceId;

        public ReserveStockFailed(DateTime createdAt, CustomerCheckout customerCheckout, string instanceId = "")
        {
            this.createdAt = createdAt;
            this.customerCheckout = customerCheckout;
            this.instanceId = instanceId;
        }

    }
}

