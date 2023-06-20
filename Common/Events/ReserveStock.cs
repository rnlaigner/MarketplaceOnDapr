using System;
using Common.Entities;
using Common.Integration;

namespace Common.Events
{
    public record ReserveStock
    (
        DateTime timestamp,
        CustomerCheckout customerCheckout,
        IList<CartItem> items,
        int instanceId
    );
}

