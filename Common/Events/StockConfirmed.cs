using System;
using Common.Entities;
using Common.Integration;
using Common.Requests;

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

