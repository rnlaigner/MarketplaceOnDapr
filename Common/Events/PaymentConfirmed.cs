using System;
using Common.Entities;
using Common.Integration;

namespace Common.Events
{
    public record PaymentConfirmed
    (
        CustomerCheckout customer,
        long orderId,
        decimal totalAmount,
        IList<OrderItem> items,
        DateTime date,
        int instanceId
    );
}

