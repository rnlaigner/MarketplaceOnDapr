using System;
using Common.Entities;

namespace Common.Events
{
    public record PaymentResult
    (
        string Status,
        CustomerCheckout? customer,
        long order_id,
        decimal? total_amount,
        IList<OrderItem>? items,
        string instanceId
    );
}

