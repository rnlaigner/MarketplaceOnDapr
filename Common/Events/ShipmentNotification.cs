using System;
using Common.Entities;

namespace Common.Events
{
    public record ShipmentNotification
    (
        string customerId,
        long orderId,
        string instanceId,
        ShipmentStatus status = ShipmentStatus.approved
    );
}

