using Common.Entities;

namespace Common.Events
{
    public record ShipmentNotification
    (
        long customerId,
        long orderId,
        DateTime eventDate,
        int instanceId,
        ShipmentStatus status = ShipmentStatus.approved
    );
}