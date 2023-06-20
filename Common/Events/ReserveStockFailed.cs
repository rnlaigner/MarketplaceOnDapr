using System;
using System.Collections.Generic;
using Common.Entities;
using Common.Integration;

namespace Common.Events
{
    public record ReserveStockFailed
    (
        DateTime timestamp,
        CustomerCheckout customerCheckout,
        IList<ProductStatus> products,
        int instanceId
    );
}

