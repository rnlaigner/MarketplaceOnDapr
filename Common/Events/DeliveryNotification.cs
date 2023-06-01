using Common.Entities;

namespace Common.Events
{
    public record DeliveryNotification
    (
        long customerId,
        long orderId,
        int packageId,
        long sellerId,
        long productId,
        string productName,
        PackageStatus status,
        DateTime deliveryDate,
        string instanceId
    );

}