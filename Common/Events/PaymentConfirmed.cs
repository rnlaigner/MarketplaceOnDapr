using System;
using Common.Entities;

namespace Common.Events
{
    public record PaymentConfirmed
    (
        CustomerCheckout customer,
        long order_id,
        decimal total_amount,
        IList<OrderItem> items,
        string instanceId
    );
}

