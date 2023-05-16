using System;
using Common.Entities;

namespace Common.Events
{
    public record DeliveryNotification
    (
        string customerId,
        long orderId,
        IEnumerable<PackageInfo> packageInfo,
        PackageStatus status,
        DateTime createdAt,
        string instanceId
    );

    public sealed class PackageInfo : Tuple<int, string>
    {
        public PackageInfo(int item1, string item2) : base(item1, item2)
        {
        }

        public int GetId() { return this.Item1; }
        // to show as notification to user without having to query the shipment ms
        public string GetProductName() { return this.Item2; }
    }
}

