using System;
using Common.Entities;

namespace Common.Events
{
    public record PaymentConfirmation
    (
        CustomerCheckout customer,
        long order_id,
        decimal total_amount,
        IList<OrderItem> items,
        string instanceId
    );
}

