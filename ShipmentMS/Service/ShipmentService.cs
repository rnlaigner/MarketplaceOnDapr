using System;
using System.Data;
using Common.Entities;
using Common.Events;
using Dapr.Client;
using ShipmentMS.Infra;
using ShipmentMS.Models;
using ShipmentMS.Repositories;

namespace ShipmentMS.Service
{
	public class ShipmentService : IShipmentService
    {
        private const string PUBSUB_NAME = "pubsub";

        private readonly IShipmentRepository shipmentRepository;
        private readonly IPackageRepository packageRepository;
        private readonly DaprClient daprClient;
        private readonly ILogger<ShipmentService> logger;

        public ShipmentService(IShipmentRepository shipmentRepository, IPackageRepository packageRepository,
                                DaprClient daprClient, ILogger<ShipmentService> logger)
        {
            this.shipmentRepository = shipmentRepository;
            this.packageRepository = packageRepository;
            this.daprClient = daprClient;
            this.logger = logger;
        }

        // https://twitter.com/hnasr/status/1657569218609684480
        [Transactional]
        public async Task ProcessShipment(PaymentConfirmed paymentResult)
        {
            int package_id = 1;

            var items = paymentResult.items
                        .GroupBy(x => x.seller_id)
                        .OrderByDescending(g => g.Count())
                        .SelectMany(x => x).ToList();

            DateTime now = DateTime.Now;

            ShipmentModel shipment = new()
            {
                order_id = paymentResult.order_id,
                customer_id = paymentResult.customer.CustomerId,
                package_count = items.Count,
                total_freight_value = items.Sum(i => i.freight_value),
                request_date = now,
                status = ShipmentStatus.approved,
                first_name = paymentResult.customer.FirstName,
                last_name = paymentResult.customer.LastName,
                street = paymentResult.customer.Street,
                complement = paymentResult.customer.Complement,
                zip_code_prefix = paymentResult.customer.ZipCode,
                city = paymentResult.customer.City,
                state = paymentResult.customer.State
            };

            shipmentRepository.Insert(shipment);

            foreach (var item in items)
            {

                PackageModel package = new()
                {
                    order_id = paymentResult.order_id,
                    package_id = package_id,
                    status = PackageStatus.shipped,
                    freight_value = item.freight_value,
                    shipping_date = now,
                    seller_id = item.seller_id,
                    product_id = item.product_id,
                    product_name = item.product_name,
                    quantity = item.quantity
                };

                packageRepository.Insert(package);
                package_id++;

            }

            // enqueue shipment notification
            ShipmentNotification shipmentNotification = new ShipmentNotification(paymentResult.customer.CustomerId, paymentResult.order_id, paymentResult.instanceId);
            await this.daprClient.PublishEventAsync(PUBSUB_NAME, nameof(ShipmentNotification), shipmentNotification);
        }

        [Transactional(IsolationLevel.Serializable)]
        public void UpdateShipment(string instanceId = "")
        {
            // perform a sql query to query the objects. write lock...
            var q = packageRepository.GetOldestOpenShipmentPerSeller();

            List<Task> tasks = new();
            foreach (var kv in q)
            {
                var packages_ = this.packageRepository.GetShippedPackagesByOrderAndSeller(kv.Value, kv.Key).ToList();
                tasks.Add(UpdatePackageDelivery(packages_, instanceId));
            }
        }

        private Task UpdatePackageDelivery(List<PackageModel> sellerPackages, string instanceId)
        {
            long orderId = sellerPackages.ElementAt(0).order_id;
            long sellerId = sellerPackages.ElementAt(0).seller_id;

            ShipmentModel? shipment = this.shipmentRepository.GetById(orderId);
            if (shipment is null) throw new Exception("Shipment ID " + orderId + " cannot be found in the database!");

            if(shipment.status == ShipmentStatus.approved)
            {
                shipment.status = ShipmentStatus.delivery_in_progress;
                this.shipmentRepository.Update(shipment);
            }

            // aggregate operation
            int countDelivered = this.packageRepository.GetTotalDeliveredPackagesForOrder(orderId);

            this.logger.LogWarning("Count delivery for shipment id {1}: {2} total of {3}",
                 shipment.order_id, countDelivered, shipment.package_count);

            List<PackageInfo> packageInfos = new(sellerPackages.Count());
            var now = DateTime.Now;
            foreach (var package in sellerPackages)
            {
                package.status = PackageStatus.delivered;
                package.delivery_date = now;

                packageInfos.Add(new PackageInfo(package.package_id, package.product_name));
            }

            DeliveryNotification deliveryNotification = new DeliveryNotification(
                shipment.customer_id, orderId, sellerId, packageInfos, PackageStatus.delivered, DateTime.Now, instanceId);

            Task task = this.daprClient.PublishEventAsync(PUBSUB_NAME, nameof(DeliveryNotification), deliveryNotification);
            
            if (shipment.package_count == countDelivered + sellerPackages.Count())
            {
                this.logger.LogWarning("Delivery concluded for shipment id {1}", shipment.order_id);
                shipment.status = ShipmentStatus.concluded;
                this.shipmentRepository.Update(shipment);
            }
            else
            {
                this.logger.LogWarning("Delivery not yet concluded for shipment id {1}: count {2} of total {3}",
                     shipment.order_id, countDelivered + sellerPackages.Count(), shipment.package_count);
            }

            return task;
        }

    }
}

