using System;
using System.Collections.Generic;
using Common.Entities;

namespace Common.Events
{
    public record ReserveStockFailed
    {
        public readonly DateTime createdAt;
        public readonly CustomerCheckout customerCheckout;
        public readonly IList<ProductStatus> products;
        public readonly string instanceId;

        public ReserveStockFailed(DateTime createdAt, CustomerCheckout customerCheckout, IList<ProductStatus> products, string instanceId = "")
        {
            this.createdAt = createdAt;
            this.customerCheckout = customerCheckout;
            this.products = products;
            this.instanceId = instanceId;
        }

    }
}

