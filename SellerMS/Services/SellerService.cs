using Common.Entities;
using Common.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SellerMS.DTO;
using SellerMS.Infra;
using SellerMS.Models;

namespace SellerMS.Services;

public class SellerService : ISellerService
{
    private readonly SellerDbContext dbContext;
    private readonly SellerConfig config;
    private readonly ILogger<SellerService> logger;

    public SellerService(SellerDbContext sellerDbContext, IOptions<SellerConfig> config, ILogger<SellerService> logger)
    {
        this.dbContext = sellerDbContext;
        this.config = config.Value;
        this.logger = logger;
    }

    private static int LOCKED = 0;

    // this method allows a natural accumulation of concurrent requests
    // thus decreasing overall cost of updating seller view
    public void RefreshSellerViewSafely()
    {
        if (0 == Interlocked.CompareExchange(ref LOCKED, 1, 0))
        {
            this.dbContext.Database.ExecuteSqlRaw(SellerDbContext.ORDER_SELLER_VIEW_UPDATE_SQL);
            Interlocked.Exchange(ref LOCKED, 0);
        }
    }

    /**
     * An order entry per seller in an order is created
     */
    public void ProcessInvoiceIssued(InvoiceIssued invoiceIssued)
    {
        using (var txCtx = this.dbContext.Database.BeginTransaction())
        {
            foreach (var item in invoiceIssued.items)
            {
                OrderEntry orderEntry = new()
                {
                    customer_id = invoiceIssued.customer.CustomerId,
                    order_id = invoiceIssued.orderId,
                    seller_id = item.seller_id,
                    product_id = item.product_id,
                    // package_id = ??? unknown at this point
                    product_name = item.product_name,
                    // should come from product
                    product_category = "",
                    unit_price = item.unit_price,
                    quantity = item.quantity,
                    total_items = item.total_items,
                    total_amount = item.total_amount,
                    total_invoice = item.total_amount + item.freight_value,
                    total_incentive = item.total_incentive,
                    freight_value = item.freight_value,
                    // shipment_date
                    // delivery_date
                    // delivery status
                    order_status = OrderStatus.INVOICED,
                    natural_key = string.Format($"{invoiceIssued.customer.CustomerId}_{invoiceIssued.orderId}")
                };
                this.dbContext.OrderEntries.Add(orderEntry);
            }
            this.dbContext.SaveChanges();
            txCtx.Commit();
        }
        this.RefreshSellerViewSafely();
    }

    public void ProcessShipmentNotification(ShipmentNotification shipmentNotification)
    {
        // create the order if does not exist. if exist, update status. using merge statement
        // https://www.postgresql.org/docs/current/sql-merge.html
        using (var txCtx = this.dbContext.Database.BeginTransaction())
        {
            var entries = this.dbContext.OrderEntries.Where(oe =>
                oe.customer_id == shipmentNotification.customerId &&
                oe.order_id == shipmentNotification.orderId).AsNoTracking();

            foreach (var oe in entries)
            {
                if (shipmentNotification.status == ShipmentStatus.approved)
                {
                    oe.order_status = OrderStatus.READY_FOR_SHIPMENT;
                    oe.shipment_date = shipmentNotification.eventDate;
                    oe.delivery_status = PackageStatus.ready_to_ship;
                    this.dbContext.OrderEntries.Update(oe);
                } else if (shipmentNotification.status == ShipmentStatus.delivery_in_progress)
                {
                    oe.order_status = OrderStatus.IN_TRANSIT;
                    oe.delivery_status = PackageStatus.shipped;
                    this.dbContext.OrderEntries.Update(oe);
                }
                else if (shipmentNotification.status == ShipmentStatus.concluded){
                    oe.order_status = OrderStatus.DELIVERED;
                }
            }

            this.dbContext.OrderEntries.UpdateRange(entries);
          
            this.dbContext.SaveChanges();
            txCtx.Commit();
        }
        this.RefreshSellerViewSafely();
    }

    /**
     * Process individual (i.e., each package at a time) delivery notifications
     */
    public void ProcessDeliveryNotification(DeliveryNotification deliveryNotification)
    {
        using (var txCtx = this.dbContext.Database.BeginTransaction())
        {
            OrderEntry? oe = this.dbContext.OrderEntries.Find(deliveryNotification.customerId, deliveryNotification.orderId, deliveryNotification.productId);
            if (oe is null) throw new Exception("[ProcessDeliveryNotification] Cannot find respective order entry for order id "+ deliveryNotification.orderId + " and product id "+ deliveryNotification.productId);

            oe.package_id = deliveryNotification.packageId;
            oe.delivery_date = deliveryNotification.deliveryDate;
            oe.delivery_status = deliveryNotification.status;

            this.dbContext.OrderEntries.Update(oe);
            this.dbContext.SaveChanges();
            txCtx.Commit();
        }
    }

    public void ProcessPaymentConfirmed(PaymentConfirmed paymentConfirmed)
    {
        Thread.Sleep(1000); // wait for order entry processing
        using (var txCtx = this.dbContext.Database.BeginTransaction())
        {
            var entries = this.dbContext.OrderEntries.Where(oe => oe.order_id == paymentConfirmed.orderId);
            foreach (var oe in entries)
            {
                oe.order_status = OrderStatus.PAYMENT_PROCESSED;
            }
            this.dbContext.OrderEntries.UpdateRange(entries);
            this.dbContext.SaveChanges();
            txCtx.Commit();  
        }
    }

    public void ProcessPaymentFailed(PaymentFailed paymentFailed)
    {
        Thread.Sleep(1000);
        using (var txCtx = this.dbContext.Database.BeginTransaction())
        {
            var entries = this.dbContext.OrderEntries.Where(oe => oe.order_id == paymentFailed.orderId);

            foreach (var oe in entries)
            {
                oe.order_status = OrderStatus.PAYMENT_FAILED;
            }

            this.dbContext.OrderEntries.UpdateRange(entries);
            this.dbContext.SaveChanges();
            txCtx.Commit();
        }
    }

    public SellerDashboard QueryDashboard(int sellerId)
    {
        using (var txCtx = this.dbContext.Database.BeginTransaction(System.Data.IsolationLevel.Snapshot))
        {
            return new SellerDashboard(
            this.dbContext.OrderSellerView.Where(v => v.seller_id == sellerId).AsEnumerable().FirstOrDefault(new OrderSellerView()),
            this.dbContext.OrderEntries.Where(oe => oe.seller_id == sellerId && (oe.order_status == OrderStatus.INVOICED || oe.order_status == OrderStatus.READY_FOR_SHIPMENT ||
                                                            oe.order_status == OrderStatus.IN_TRANSIT || oe.order_status == OrderStatus.PAYMENT_PROCESSED)).ToList()
            );
        }
    }

    // cleanup cleans up the database
    public void Cleanup()
    {
        this.dbContext.Sellers.ExecuteDelete();
        this.dbContext.OrderEntries.ExecuteDelete();
        this.dbContext.SaveChanges();
        this.dbContext.Database.ExecuteSqlRaw(SellerDbContext.ORDER_SELLER_VIEW_UPDATE_SQL);
    }

    // reset maintains seller records
    public void Reset()
    {
        this.dbContext.OrderEntries.ExecuteDelete();
        this.dbContext.SaveChanges();
        this.dbContext.Database.ExecuteSqlRaw(SellerDbContext.ORDER_SELLER_VIEW_UPDATE_SQL);
    }

}
