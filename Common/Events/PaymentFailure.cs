using System;
using Common.Entities;

namespace Common.Events
{
    public record PaymentFailure
    (
        string Status,
        CustomerCheckout customer,
        long order_id,
        IList<OrderItem> items,
        decimal total_amount,
        string instanceId
    );
}

