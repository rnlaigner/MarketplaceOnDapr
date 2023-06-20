using System;
using Common.Entities;
using Common.Integration;

namespace Common.Events
{
    public record StockConfirmed
    (
        DateTime timestamp,
        CustomerCheckout customerCheckout,
        List<CartItem> items,
        int instanceId
    );
}

