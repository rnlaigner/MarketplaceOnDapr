using System;
using Common.Entities;

namespace Common.Events
{
    // an invoice is a request for payment
    public record PaymentRequest
    (
        CustomerCheckout customer,
        long order_id,
        decimal total_amount,
        IList<OrderItem> items,
        string instanceId
    );
}

