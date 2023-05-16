using System;
using Common.Entities;

namespace Common.Events
{
    public record PaymentFailure
    (
        string Status,
        CustomerCheckout customer,
        long order_id,
        decimal total_amount,
        string instanceId
    );
}

