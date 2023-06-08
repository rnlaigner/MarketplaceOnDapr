using System;
using System.Collections.Generic;
using Common.Entities;

namespace Common.Events
{
    public record ReserveStockFailed
    (
        DateTime timestamp,
        CustomerCheckout customerCheckout,
        IList<ProductStatus> products,
        string instanceId = ""
    );
}

