using System;
using Common.Entities;

namespace Common.Events
{
    public record CheckoutNotification
    (
        string customerId,
        string instanceId
    );
}

