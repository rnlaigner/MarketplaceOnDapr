using Common.Entities;
using Common.Events;
using Dapr.Client;
using SellerMS.DTO;
using SellerMS.Infra;
using SellerMS.Models;
using SellerMS.Repositories;

namespace SellerMS.Services
{
	public class SellerService : ISellerService
	{

        private readonly DaprClient daprClient;
        private readonly SellerDbContext dbContext;
        private readonly ISellerRepository sellerRepository;
        private readonly ILogger<SellerService> logger;

        public SellerService(DaprClient daprClient, SellerDbContext sellerDbContext, ISellerRepository sellerRepository, ILogger<SellerService> logger)
		{
            this.daprClient = daprClient;
            this.dbContext = sellerDbContext;
            this.sellerRepository = sellerRepository;
            this.logger = logger;
        }

        public async Task ProcessShipmentNotification(ShipmentNotification shipmentNotification)
        {
            // create the order if does not exist. if exist, update status. using merge statement
            // https://www.postgresql.org/docs/current/sql-merge.html
            using (var txCtx = dbContext.Database.BeginTransaction())
            {
                var entries = dbContext.OrderEntries.Where(oe=> oe.order_id == shipmentNotification.orderId);

                OrderStatus? orderStatus = null;
                if (shipmentNotification.status == ShipmentStatus.approved) orderStatus = OrderStatus.READY_FOR_SHIPMENT;
                if (shipmentNotification.status == ShipmentStatus.delivery_in_progress) orderStatus = OrderStatus.IN_TRANSIT;
                if (shipmentNotification.status == ShipmentStatus.concluded) orderStatus = OrderStatus.DELIVERED;

                foreach (var oe in entries)
                {
                    if(orderStatus is not null)
                        oe.order_status = orderStatus.Value;
                    if(shipmentNotification.status == ShipmentStatus.delivery_in_progress)
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

                await txCtx.CommitAsync();

            }
        }

        /**
         * Process individual (i.e., each package at a time) delivery notifications
         */
        public async Task ProcessDeliveryNotification(DeliveryNotification deliveryNotification)
        {
            using (var txCtx = dbContext.Database.BeginTransaction())
            {
                OrderEntry? oe = dbContext.OrderEntries.Find(deliveryNotification.orderId, deliveryNotification.productId);

                if (oe is not null)
                {
                    oe.package_id = deliveryNotification.packageId;
                    oe.delivery_date = deliveryNotification.deliveryDate;
                    oe.delivery_status = deliveryNotification.status;

                    dbContext.OrderEntries.Update(oe);

                    dbContext.SaveChanges();

                    await txCtx.CommitAsync();
                } else
                {
                    logger.LogError("[DeliveryNotification] Cannot find respective order entry for order id {0} and product id {1}", deliveryNotification.orderId, deliveryNotification.productId);
                }

            }
        }

        /**
         * An order entry per seller in an order is created
         * 
         */
        public async Task ProcessNewInvoice(InvoiceIssued invoiceIssued)
        {

            using (var txCtx = dbContext.Database.BeginTransaction())
            {
                var sellerGroups = invoiceIssued.items
                                        .GroupBy(x => x.seller_id)
                                        .ToDictionary(k => k.Key, v => v.ToList());

                foreach (var sellerGroup in sellerGroups)
                {
                    var sellerItems = sellerGroup.Value;

                    // should this be calculated by the view or here?
                    // from a point of view of the seller, the seller is
                    // interested in the overview of own items, not the overall order data

                    /*
                    OrderSellerView oevm = new()
                    {
                        order_id = invoiceIssued.order_id,
                        seller_id = sellerGroup.Key,
                        count_items = items.Count(),
                        total_amount = items.Sum(i => i.total_amount),
                        total_freight = items.Sum(i => i.freight_value),
                        total_incentive = items.Sum(i => i.total_items - i.total_amount),
                        total_invoice = items.Sum(i => i.total_amount + i.freight_value),
                        total_items = items.Sum(i => i.total_items)
                    };
                    */

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
                            freight_value = item.freight_value,
                            // shipment_date
                            // delivery_date
                            order_status = OrderStatus.INVOICED
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

                await txCtx.CommitAsync();
            }
        }

        public async Task ProcessPaymentConfirmed(PaymentConfirmed paymentConfirmed)
        {
            using (var txCtx = dbContext.Database.BeginTransaction())
            {
                var entries = dbContext.OrderEntries.Where(oe => oe.order_id == paymentConfirmed.orderId);

                foreach (var oe in entries)
                {
                    oe.order_status = OrderStatus.PAYMENT_PROCESSED;
                }

                dbContext.OrderEntries.UpdateRange(entries);
                
                OrderEntryDetails? oed = dbContext.OrderEntryDetails.Find(paymentConfirmed.orderId);

                if(oed is not null)
                {
                    oed.status = OrderStatus.PAYMENT_PROCESSED;
                    dbContext.OrderEntryDetails.Update(oed);
                    dbContext.SaveChanges();
                    await txCtx.CommitAsync();
                    
                }
            }
        }

        public async Task ProcessPaymentFailed(PaymentFailed paymentFailed)
        {
            using (var txCtx = dbContext.Database.BeginTransaction())
            {
                var entries = dbContext.OrderEntries.Where(oe => oe.order_id == paymentFailed.orderId);

                foreach (var oe in entries)
                {
                    oe.order_status = OrderStatus.PAYMENT_FAILED;
                }

                dbContext.OrderEntries.UpdateRange(entries);

                OrderEntryDetails? oed = dbContext.OrderEntryDetails.Find(paymentFailed.orderId);

                if (oed is not null)
                {
                    oed.status = OrderStatus.PAYMENT_FAILED;
                    dbContext.OrderEntryDetails.Update(oed);
                }

                dbContext.SaveChanges();
                await txCtx.CommitAsync();
            }
        }

        public Task ProcessProductUpdate(Product product)
        {
            // throw new NotImplementedException();
            logger.LogInformation("[ProcessProductUpdate] Product ID {0}", product.id);
            return Task.CompletedTask;
        }

        public Task ProcessStockItem(StockItem stockItem)
        {
            // throw new NotImplementedException();
            logger.LogInformation("[ProcessStockItem] Product ID {0}", stockItem.product_id);
            return Task.CompletedTask;
        }

        /**
         * Query results may differ among DBMS calls due to concurrent events being processed
         */
        public SellerDashboard QueryDashboard(long sellerId)
        {
            // perhaps Find(sellerId) works... have to test
            return new SellerDashboard(
                dbContext.OrderSellerView.Where(v => v.seller_id == sellerId).FirstOrDefault(),
                dbContext.OrderEntries.Where(oe=>oe.seller_id == sellerId && ( oe.order_status == OrderStatus.INVOICED || oe.order_status == OrderStatus.READY_FOR_SHIPMENT ||
                                                                                oe.order_status == OrderStatus.IN_TRANSIT || oe.order_status == OrderStatus.PAYMENT_PROCESSED ) ).ToList()      
                // dbContext.ProductEntries.Where(pe=>pe.seller_id == sellerId).OrderBy(pe=>pe.order_count).Take(10).ToList()
                );
            
        }
    }
}

