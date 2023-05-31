using System;
using Common.Entities;

namespace Common.Events
{
    public record PaymentConfirmed
    (
        CustomerCheckout customer,
        long orderId,
        decimal totalAmount,
        IList<OrderItem> items,
        string instanceId
    );
}

