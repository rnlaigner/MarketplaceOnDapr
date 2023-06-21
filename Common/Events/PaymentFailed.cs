using System;
using Common.Entities;
using Common.Integration;
using Common.Requests;

namespace Common.Events
{
    public record PaymentFailed
    (
        string status,
        CustomerCheckout customer,
        long orderId,
        IList<OrderItem> items,
        decimal totalAmount,
        int instanceId
    );
}

