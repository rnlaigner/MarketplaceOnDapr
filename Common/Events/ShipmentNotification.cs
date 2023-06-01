using System;
using Common.Entities;

namespace Common.Events
{
    public record ShipmentNotification
    (
        long customerId,
        long orderId,
        DateTime eventDate,
        string instanceId,
        ShipmentStatus status = ShipmentStatus.approved
    );
}