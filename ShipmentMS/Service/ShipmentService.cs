using System;
using Common.Entities;
using Common.Events;
using ShipmentMS.Infra;
using ShipmentMS.Models;
using ShipmentMS.Repositories;

namespace ShipmentMS.Service
{
	public class ShipmentService : IShipmentService
    {

        private readonly IShipmentRepository shipmentRepository;
        private readonly IPackageRepository packageRepository;
        private readonly ILogger<ShipmentService> logger;

        public ShipmentService(IShipmentRepository shipmentRepository, IPackageRepository packageRepository, ILogger<ShipmentService> logger)
        {
            this.shipmentRepository = shipmentRepository;
            this.packageRepository = packageRepository;
            this.logger = logger;
        }

        [Transactional]
        public void ProcessShipment(PaymentResult paymentResult)
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
                    quantity = item.quantity
                };

                packageRepository.Insert(package);
                package_id++;

            }

            // TODO enqueue shipment notification

        }

        [Transactional]
        public void UpdateShipment()
        {
            // TODO perform a raw sql query to query the objects. write lock... 
        }


    }
}

