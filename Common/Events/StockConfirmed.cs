using System;
using Common.Entities;

namespace Common.Events
{
    public record StockConfirmed
    (
        DateTime timestamp,
        CustomerCheckout customerCheckout,
        List<CartItem> items,
        string instanceId = ""
    );
}

