using System.Data;
using System.Text;
using Common.Driver;
using Common.Entities;
using Common.Events;
using Dapr.Client;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using ShipmentMS.Infra;
using ShipmentMS.Models;
using ShipmentMS.Repositories;

namespace ShipmentMS.Service;

public class ShipmentService : IShipmentService
{
    private const string PUBSUB_NAME = "pubsub";

    private readonly ShipmentDbContext dbContext;
    private readonly IShipmentRepository shipmentRepository;
    private readonly IPackageRepository packageRepository;
    private readonly ShipmentConfig config;
    private readonly DaprClient daprClient;
    private readonly ILogger<ShipmentService> logger;

    public ShipmentService(ShipmentDbContext dbContext, IShipmentRepository shipmentRepository, IPackageRepository packageRepository, IOptions<ShipmentConfig> config, DaprClient daprClient, ILogger<ShipmentService> logger)
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

            DateTime now = DateTime.UtcNow;

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

            dbContext.SaveChanges();
            txCtx.Commit();

            // enqueue shipment notification
            if (config.ShipmentStreaming)
            {
                ShipmentNotification shipmentNotification = new ShipmentNotification(paymentConfirmed.customer.CustomerId, paymentConfirmed.orderId, now, paymentConfirmed.instanceId);
                await Task.WhenAll(
                    this.daprClient.PublishEventAsync(PUBSUB_NAME, nameof(ShipmentNotification), shipmentNotification),
                    // publish transaction event result
                    this.daprClient.PublishEventAsync(PUBSUB_NAME, streamId, new TransactionMark(paymentConfirmed.instanceId, TransactionType.CUSTOMER_SESSION, paymentConfirmed.customer.CustomerId, MarkStatus.SUCCESS, "shipment"))
                );
            }

        }
    }

    static readonly string streamId = new StringBuilder(nameof(TransactionMark)).Append('_').Append(TransactionType.CUSTOMER_SESSION.ToString()).ToString();

    public async Task ProcessPoisonShipment(PaymentConfirmed paymentConfirmed)
    {
        await this.daprClient.PublishEventAsync(PUBSUB_NAME, streamId, new TransactionMark(paymentConfirmed.instanceId, TransactionType.CUSTOMER_SESSION, paymentConfirmed.customer.CustomerId, MarkStatus.ABORT, "shipment"));
    }

    // update delivery status of many packages
    public async Task UpdateShipment(int instanceId)
    {
        using (var txCtx = dbContext.Database.BeginTransaction(IsolationLevel.Serializable))
        {
            // perform a sql query to query the objects. write lock...
            var q = packageRepository.GetOldestOpenShipmentPerSeller();

            foreach (var kv in q)
            {
                // logger.LogWarning("Seller ID {0}", kv.Key);
                var packages_ = this.packageRepository.GetShippedPackagesByOrderAndSeller(kv.Value, kv.Key).ToList();
                if (packages_.Count() == 0)
                {
                    logger.LogWarning("No packages retrieved from the DB for seller {0}", kv.Key);
                    continue;
                }
                await UpdatePackageDelivery(packages_, instanceId);
            }

            txCtx.Commit();
        }
    }

    private async Task UpdatePackageDelivery(List<PackageModel> sellerPackages, int instanceId)
    {
        int orderId = sellerPackages.ElementAt(0).order_id;
        int sellerId = sellerPackages.ElementAt(0).seller_id;

        ShipmentModel? shipment = this.shipmentRepository.GetById(orderId);
        if (shipment is null) throw new Exception("Shipment ID " + orderId + " cannot be found in the database!");
        List<Task> tasks = new(sellerPackages.Count() + 1);
        var now = DateTime.UtcNow;
        if (shipment.status == ShipmentStatus.approved)
        {
            shipment.status = ShipmentStatus.delivery_in_progress;
            this.shipmentRepository.Update(shipment);
            this.shipmentRepository.Save();
            ShipmentNotification shipmentNotification = new ShipmentNotification(
                    shipment.customer_id, shipment.order_id, now, instanceId, ShipmentStatus.delivery_in_progress);
            tasks.Add(this.daprClient.PublishEventAsync(PUBSUB_NAME, nameof(ShipmentNotification), shipmentNotification));
        }

        // aggregate operation
        int countDelivered = this.packageRepository.GetTotalDeliveredPackagesForOrder(orderId);

        this.logger.LogDebug("Count delivery for shipment id {1}: {2} total of {3}",
                shipment.order_id, countDelivered, shipment.package_count);

        foreach (var package in sellerPackages)
        {
            package.status = PackageStatus.delivered;
            package.delivery_date = now;
            this.packageRepository.Update(package);
            var delivery = new DeliveryNotification(
                shipment.customer_id, package.order_id, package.package_id, package.seller_id,
                package.product_id, package.product_name, PackageStatus.delivered, now, instanceId);

            tasks.Add(this.daprClient.PublishEventAsync(PUBSUB_NAME, nameof(DeliveryNotification), delivery));
        }
        this.packageRepository.Save();

        if (shipment.package_count == countDelivered + sellerPackages.Count())
        {
            this.logger.LogDebug("Delivery concluded for shipment id {1}", shipment.order_id);
            shipment.status = ShipmentStatus.concluded;
            this.shipmentRepository.Update(shipment);
            this.shipmentRepository.Save();
            ShipmentNotification shipmentNotification = new ShipmentNotification(
                shipment.customer_id, shipment.order_id, now, instanceId, ShipmentStatus.concluded);
            tasks.Add(this.daprClient.PublishEventAsync(PUBSUB_NAME, nameof(ShipmentNotification), shipmentNotification));
        }
        else
        {
            this.logger.LogDebug("Delivery not yet concluded for shipment id {1}: count {2} of total {3}",
                    shipment.order_id, countDelivered + sellerPackages.Count(), shipment.package_count);
        }

        await Task.WhenAll(tasks);
    }

    public void Cleanup()
    {
        this.dbContext.Packages.ExecuteDelete();
        this.dbContext.Shipments.ExecuteDelete();
        this.dbContext.SaveChanges();
    }

}

