using System;
using Common.Entities;

namespace Common.Events
{
    public record ReserveStock
    (
         DateTime timestamp,
         CustomerCheckout customerCheckout,
         IList<CartItem> items,
         string instanceId = "" );
}

