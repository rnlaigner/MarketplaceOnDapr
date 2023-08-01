using Common.Entities;
using Common.Events;
using Microsoft.EntityFrameworkCore;
using SellerMS.DTO;
using SellerMS.Infra;
using SellerMS.Models;

namespace SellerMS.Services
{
    public class SellerService : ISellerService
    {

        private readonly SellerDbContext dbContext;
        private readonly ILogger<SellerService> logger;

        public SellerService(SellerDbContext sellerDbContext, ILogger<SellerService> logger)
        {
            this.dbContext = sellerDbContext;
            this.logger = logger;
        }

        public void ProcessShipmentNotification(ShipmentNotification shipmentNotification)
        {
            // create the order if does not exist. if exist, update status. using merge statement
            // https://www.postgresql.org/docs/current/sql-merge.html

            using (var txCtx = dbContext.Database.BeginTransaction())
            {
                var entries = dbContext.OrderEntries.Where(oe => oe.order_id == shipmentNotification.orderId);

                OrderStatus? orderStatus = null;
                if (shipmentNotification.status == ShipmentStatus.approved) orderStatus = OrderStatus.READY_FOR_SHIPMENT;
                if (shipmentNotification.status == ShipmentStatus.delivery_in_progress) orderStatus = OrderStatus.IN_TRANSIT;
                if (shipmentNotification.status == ShipmentStatus.concluded) orderStatus = OrderStatus.DELIVERED;

                foreach (var oe in entries)
                {
                    if (orderStatus is not null)
                        oe.order_status = orderStatus.Value;
                    if (shipmentNotification.status == ShipmentStatus.delivery_in_progress)
                        oe.shipment_date = shipmentNotification.eventDate;
                }

                dbContext.OrderEntries.UpdateRange(entries);

                OrderEntryDetails? oed = dbContext.OrderEntryDetails.Find(shipmentNotification.orderId);
                if (orderStatus is not null && oed is not null)
                {
                    oed.status = orderStatus.Value;
                    dbContext.OrderEntryDetails.Update(oed);
                }

                dbContext.SaveChanges();
                txCtx.Commit();

            }

        }

        /**
         * Process individual (i.e., each package at a time) delivery notifications
         */
        public void ProcessDeliveryNotification(DeliveryNotification deliveryNotification)
        {
            using (var txCtx = dbContext.Database.BeginTransaction())
            {
                OrderEntry? oe = dbContext.OrderEntries.Find(deliveryNotification.orderId, deliveryNotification.productId);
                if (oe is null) throw new Exception("[ProcessDeliveryNotification] Cannot find respective order entry for order id "+ deliveryNotification.orderId + " and product id "+ deliveryNotification.productId);

                oe.package_id = deliveryNotification.packageId;
                oe.delivery_date = deliveryNotification.deliveryDate;
                oe.delivery_status = deliveryNotification.status;

                dbContext.OrderEntries.Update(oe);
                dbContext.SaveChanges();
                txCtx.Commit();
            }
        }

        /**
         * An order entry per seller in an order is created
         * 
         */
        public void ProcessNewInvoice(InvoiceIssued invoiceIssued)
        {

            using (var txCtx = dbContext.Database.BeginTransaction())
            {
                var sellerGroups = invoiceIssued.items
                                        .GroupBy(x => x.seller_id)
                                        .ToDictionary(k => k.Key, v => v.ToList());

                foreach (var sellerGroup in sellerGroups)
                {
                    var sellerItems = sellerGroup.Value;
                    foreach (var item in sellerItems)
                    {
                        OrderEntry orderEntry = new()
                        {
                            order_id = invoiceIssued.orderId,
                            seller_id = sellerGroup.Key,
                            // package_id =
                            product_id = item.product_id,
                            product_name = item.product_name,
                            quantity = item.quantity,
                            total_amount = item.total_amount,
                            total_items = item.total_items,
                            total_invoice = item.total_amount + item.freight_value,
                            total_incentive = item.total_items - item.total_amount,
                            freight_value = item.freight_value,
                            // shipment_date
                            // delivery_date
                            order_status = OrderStatus.INVOICED,
                            unit_price = item.unit_price,
                            // product_category = ? should come from product
                        };

                        dbContext.OrderEntries.Add(orderEntry);

                    }
                }

                // order details
                OrderEntryDetails oed = new()
                {
                    order_id = invoiceIssued.orderId,
                    order_date = invoiceIssued.issueDate,
                    status = OrderStatus.INVOICED,
                    customer_id = invoiceIssued.customer.CustomerId,
                    first_name = invoiceIssued.customer.FirstName,
                    last_name = invoiceIssued.customer.LastName,
                    street = invoiceIssued.customer.Street,
                    complement = invoiceIssued.customer.Complement,
                    zip_code = invoiceIssued.customer.ZipCode,
                    city = invoiceIssued.customer.City,
                    state = invoiceIssued.customer.State,
                    card_brand = invoiceIssued.customer.CardBrand,
                    installments = invoiceIssued.customer.Installments
                };

                dbContext.OrderEntryDetails.Add(oed);
                dbContext.SaveChanges();

                txCtx.Commit();
            }

        }

        public void ProcessPaymentConfirmed(PaymentConfirmed paymentConfirmed)
        {
            Thread.Sleep(1000); // wait for order entry processing
            using (var txCtx = dbContext.Database.BeginTransaction())
            {
                OrderEntryDetails? oed = dbContext.OrderEntryDetails.Find(paymentConfirmed.orderId);
                if (oed is null) throw new Exception("[ProcessPaymentConfirmed] Cannot find corresponding order entry " + paymentConfirmed.orderId);

                var entries = dbContext.OrderEntries.Where(oe => oe.order_id == paymentConfirmed.orderId);

                foreach (var oe in entries)
                {
                    oe.order_status = OrderStatus.PAYMENT_PROCESSED;
                }

                dbContext.OrderEntries.UpdateRange(entries);

                oed.status = OrderStatus.PAYMENT_PROCESSED;
                dbContext.OrderEntryDetails.Update(oed);
                dbContext.SaveChanges();
                txCtx.Commit();  
            }

        }

        public void ProcessPaymentFailed(PaymentFailed paymentFailed)
        {
            Thread.Sleep(1000);
            using (var txCtx = dbContext.Database.BeginTransaction())
            {

                OrderEntryDetails? oed = dbContext.OrderEntryDetails.Find(paymentFailed.orderId);
                if (oed is null) throw new Exception("[ProcessPaymentConfirmed] Cannot find corresponding order entry " + paymentFailed.orderId);

                var entries = dbContext.OrderEntries.Where(oe => oe.order_id == paymentFailed.orderId);

                foreach (var oe in entries)
                {
                    oe.order_status = OrderStatus.PAYMENT_FAILED;
                }

                dbContext.OrderEntries.UpdateRange(entries);

                oed.status = OrderStatus.PAYMENT_FAILED;
                dbContext.OrderEntryDetails.Update(oed);
                
                dbContext.SaveChanges();
                txCtx.Commit();
            }

        }

        public void ProcessProductUpdate(Product product)
        {
            logger.LogInformation("[ProcessProductUpdate] Product ID {0}", product.product_id);
        }

        public void ProcessStockItem(StockItem stockItem)
        {
            logger.LogInformation("[ProcessStockItem] Product ID {0}", stockItem.product_id);
        }

        public SellerDashboard QueryDashboard(int sellerId)
        {
            using (var txCtx = dbContext.Database.BeginTransaction())
            {
                return new SellerDashboard(
                dbContext.OrderSellerView.Where(v => v.seller_id == sellerId).FirstOrDefault(new OrderSellerView()),
                dbContext.OrderEntries.Where(oe => oe.seller_id == sellerId && (oe.order_status == OrderStatus.INVOICED || oe.order_status == OrderStatus.READY_FOR_SHIPMENT ||
                                                               oe.order_status == OrderStatus.IN_TRANSIT || oe.order_status == OrderStatus.PAYMENT_PROCESSED)).ToList()
                );
            }
        }

        // cleanup cleans up the database
        public void Cleanup()
        {
            this.dbContext.Sellers.ExecuteDelete();
            this.dbContext.OrderEntries.ExecuteDelete();
            this.dbContext.OrderEntryDetails.ExecuteDelete();
            this.dbContext.SaveChanges();
            this.dbContext.Database.ExecuteSqlRaw($"REFRESH MATERIALIZED VIEW CONCURRENTLY {nameof(OrderSellerView)};");
        }

        // reset maintains seller records
        public void Reset()
        {
            this.dbContext.OrderEntries.ExecuteDelete();
            this.dbContext.OrderEntryDetails.ExecuteDelete();
            this.dbContext.SaveChanges();
            this.dbContext.Database.ExecuteSqlRaw($"REFRESH MATERIALIZED VIEW CONCURRENTLY {nameof(OrderSellerView)};");
        }

    }
}