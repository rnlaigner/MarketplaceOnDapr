using System;
using Common.Entities;

namespace Common.Events
{
    public record CheckoutNotification
    (
        long customerId,
        string instanceId
    );
}

