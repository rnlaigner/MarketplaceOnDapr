﻿using System;
using System.Data;
using Common.Entities;
using Common.Events;
using Dapr.Client;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using ShipmentMS.Infra;
using ShipmentMS.Models;
using ShipmentMS.Repositories;

namespace ShipmentMS.Service
{
	public class ShipmentService : IShipmentService
    {
        private const string PUBSUB_NAME = "pubsub";

        private readonly ShipmentDbContext dbContext;
        private readonly IShipmentRepository shipmentRepository;
        private readonly IPackageRepository packageRepository;
        private readonly ShipmentConfig config;
        private readonly DaprClient daprClient;
        private readonly ILogger<ShipmentService> logger;

        public ShipmentService(ShipmentDbContext dbContext, IShipmentRepository shipmentRepository, IPackageRepository packageRepository, IOptions<ShipmentConfig> config,
                                DaprClient daprClient, ILogger<ShipmentService> logger)
        {
            this.dbContext = dbContext;
            this.shipmentRepository = shipmentRepository;
            this.packageRepository = packageRepository;
            this.config = config.Value;
            this.daprClient = daprClient;
            this.logger = logger;
        }

        /*
         * https://twitter.com/hnasr/status/1657569218609684480
         */
        public async Task ProcessShipment(PaymentConfirmed paymentConfirmed)
        {
            using (var txCtx = dbContext.Database.BeginTransaction())
            {
                int package_id = 1;

                var items = paymentConfirmed.items
                            .GroupBy(x => x.seller_id)
                            .OrderByDescending(g => g.Count())
                            .SelectMany(x => x).ToList();

                DateTime now = DateTime.Now;

                ShipmentModel shipment = new()
                {
                    order_id = paymentConfirmed.orderId,
                    customer_id = paymentConfirmed.customer.CustomerId,
                    package_count = items.Count,
                    total_freight_value = items.Sum(i => i.freight_value),
                    request_date = now,
                    status = ShipmentStatus.approved,
                    first_name = paymentConfirmed.customer.FirstName,
                    last_name = paymentConfirmed.customer.LastName,
                    street = paymentConfirmed.customer.Street,
                    complement = paymentConfirmed.customer.Complement,
                    zip_code = paymentConfirmed.customer.ZipCode,
                    city = paymentConfirmed.customer.City,
                    state = paymentConfirmed.customer.State
                };

                this.shipmentRepository.Insert(shipment);

                foreach (var item in items)
                {
                    PackageModel package = new()
                    {
                        order_id = paymentConfirmed.orderId,
                        package_id = package_id,
                        status = PackageStatus.shipped,
                        freight_value = item.freight_value,
                        shipping_date = now,
                        seller_id = item.seller_id,
                        product_id = item.product_id,
                        product_name = item.product_name,
                        quantity = item.quantity
                    };
                    this.packageRepository.Insert(package);
                    package_id++;
                }

                // enqueue shipment notification
                if (config.ShipmentStreaming)
                {
                    ShipmentNotification shipmentNotification = new ShipmentNotification(paymentConfirmed.customer.CustomerId, paymentConfirmed.orderId, now, paymentConfirmed.instanceId);
                    await this.daprClient.PublishEventAsync(PUBSUB_NAME, nameof(ShipmentNotification), shipmentNotification);
                }

                dbContext.SaveChanges();
                txCtx.Commit();

                // TODO publish transactional event result
            }
        }

        public async Task UpdateShipment(int instanceId)
        {
            using (var txCtx = dbContext.Database.BeginTransaction())
            {
                // perform a sql query to query the objects. write lock...
                var q = packageRepository.GetOldestOpenShipmentPerSeller();

                List<Task> tasks = new();
                foreach (var kv in q)
                {
                    var packages_ = this.packageRepository.GetShippedPackagesByOrderAndSeller(kv.Value, kv.Key).ToList();
                    tasks.Add(UpdatePackageDelivery(packages_, instanceId));
                }

                await Task.WhenAll(tasks);

                dbContext.SaveChanges();
                txCtx.Commit();

            }
        }

        private async Task UpdatePackageDelivery(List<PackageModel> sellerPackages, int instanceId)
        {
            long orderId = sellerPackages.ElementAt(0).order_id;
            long sellerId = sellerPackages.ElementAt(0).seller_id;

            ShipmentModel? shipment = this.shipmentRepository.GetById(orderId);
            if (shipment is null) throw new Exception("Shipment ID " + orderId + " cannot be found in the database!");

            var now = DateTime.Now;
            if (shipment.status == ShipmentStatus.approved)
            {
                shipment.status = ShipmentStatus.delivery_in_progress;
                this.shipmentRepository.Update(shipment);
                ShipmentNotification shipmentNotification = new ShipmentNotification(
                        shipment.customer_id, shipment.order_id, now, instanceId, shipment.status);
                await this.daprClient.PublishEventAsync(PUBSUB_NAME, nameof(ShipmentNotification), shipmentNotification);
            }

            // aggregate operation
            int countDelivered = this.packageRepository.GetTotalDeliveredPackagesForOrder(orderId);

            this.logger.LogWarning("Count delivery for shipment id {1}: {2} total of {3}",
                 shipment.order_id, countDelivered, shipment.package_count);

            List<Task> tasks = new(sellerPackages.Count());
            foreach (var package in sellerPackages)
            {
                package.status = PackageStatus.delivered;
                package.delivery_date = now;

                var delivery = new DeliveryNotification(
                    shipment.customer_id, package.order_id, package.package_id, package.seller_id,
                    package.product_id, package.product_name, PackageStatus.delivered, now, instanceId);

                tasks.Add(this.daprClient.PublishEventAsync(PUBSUB_NAME, nameof(DeliveryNotification), delivery));
            }

            if (shipment.package_count == countDelivered + sellerPackages.Count())
            {
                this.logger.LogWarning("Delivery concluded for shipment id {1}", shipment.order_id);
                shipment.status = ShipmentStatus.concluded;
                this.shipmentRepository.Update(shipment);

                ShipmentNotification shipmentNotification = new ShipmentNotification(
                    shipment.customer_id, shipment.order_id, now, instanceId, ShipmentStatus.concluded);
                tasks.Add( this.daprClient.PublishEventAsync(PUBSUB_NAME, nameof(ShipmentNotification), shipmentNotification) );
            }
            else
            {
                this.logger.LogWarning("Delivery not yet concluded for shipment id {1}: count {2} of total {3}",
                     shipment.order_id, countDelivered + sellerPackages.Count(), shipment.package_count);
            }

            await Task.WhenAll(tasks);
        }

    }
}

