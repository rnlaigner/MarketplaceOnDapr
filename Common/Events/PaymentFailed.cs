using System;
using Common.Entities;

namespace Common.Events
{
    public record PaymentFailed
    (
        string status,
        CustomerCheckout customer,
        long orderId,
        IList<OrderItem> items,
        decimal totalAmount,
        string instanceId
    );
}

