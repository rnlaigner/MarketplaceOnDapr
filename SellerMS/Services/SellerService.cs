using Common.Entities;
using Common.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SellerMS.DTO;
using SellerMS.Infra;
using SellerMS.Models;
using SellerMS.Repositories;

namespace SellerMS.Services;

public class SellerService : ISellerService
{
    private readonly ISellerRepository sellerRepository;
    private readonly SellerConfig config;
    private readonly ILogger<SellerService> logger;

    public SellerService(ISellerRepository sellerRepository, IOptions<SellerConfig> config, ILogger<SellerService> logger)
    {
        this.sellerRepository = sellerRepository;
        this.config = config.Value;
        this.logger = logger;
    }

    /**
     * An order entry per seller in an order is created
     */
    public void ProcessInvoiceIssued(InvoiceIssued invoiceIssued)
    {
        using (var txCtx = this.sellerRepository.BeginTransaction())
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
                this.sellerRepository.AddOrderEntry(orderEntry);
            }
            this.sellerRepository.FlushUpdates();
            txCtx.Commit();
        }
        this.sellerRepository.RefreshSellerViewSafely();
    }

    public void ProcessShipmentNotification(ShipmentNotification shipmentNotification)
    {
        // create the order if does not exist. if exist, update status. using merge statement
        // https://www.postgresql.org/docs/current/sql-merge.html
        using (var txCtx = this.sellerRepository.BeginTransaction())
        {
            var entries = this.sellerRepository.GetOrderEntries(shipmentNotification.customerId, shipmentNotification.orderId);
            foreach (var oe in entries)
            {
                if (shipmentNotification.status == ShipmentStatus.approved)
                {
                    oe.order_status = OrderStatus.READY_FOR_SHIPMENT;
                    oe.shipment_date = shipmentNotification.eventDate;
                    oe.delivery_status = PackageStatus.ready_to_ship;
                } else if (shipmentNotification.status == ShipmentStatus.delivery_in_progress)
                {
                    oe.order_status = OrderStatus.IN_TRANSIT;
                    oe.delivery_status = PackageStatus.shipped;
                }
                else if (shipmentNotification.status == ShipmentStatus.concluded){
                    oe.order_status = OrderStatus.DELIVERED;
                }
            }
            this.sellerRepository.UpdateRange(entries);
            this.sellerRepository.FlushUpdates();
            txCtx.Commit();
        }
        this.sellerRepository.RefreshSellerViewSafely();
    }

    /**
     * Process individual (i.e., each package at a time) delivery notifications
     */
    public void ProcessDeliveryNotification(DeliveryNotification deliveryNotification)
    {
        using (var txCtx = this.sellerRepository.BeginTransaction())
        {
            OrderEntry? oe = this.sellerRepository.Find(
                deliveryNotification.customerId, 
                deliveryNotification.orderId,
                deliveryNotification.sellerId,
                deliveryNotification.productId);
            if (oe is null) throw new Exception("[ProcessDeliveryNotification] Cannot find respective order entry for order id "+ deliveryNotification.orderId + " and product id "+ deliveryNotification.productId);

            oe.package_id = deliveryNotification.packageId;
            oe.delivery_date = deliveryNotification.deliveryDate;
            oe.delivery_status = deliveryNotification.status;

            this.sellerRepository.Update(oe);
            this.sellerRepository.FlushUpdates();
            txCtx.Commit();
        }
    }

    public void ProcessPaymentConfirmed(PaymentConfirmed paymentConfirmed)
    {
        using (var txCtx = this.sellerRepository.BeginTransaction())
        {
            var entries = this.sellerRepository.GetOrderEntries(paymentConfirmed.customer.CustomerId, paymentConfirmed.orderId);
            foreach (var oe in entries)
            {
                oe.order_status = OrderStatus.PAYMENT_PROCESSED;
            }
            this.sellerRepository.UpdateRange(entries);
            this.sellerRepository.FlushUpdates();
            txCtx.Commit();  
        }
    }

    public void ProcessPaymentFailed(PaymentFailed paymentFailed)
    {
        using (var txCtx = this.sellerRepository.BeginTransaction())
        {
            var entries = this.sellerRepository.GetOrderEntries(paymentFailed.customer.CustomerId, paymentFailed.orderId);
            foreach (var oe in entries)
            {
                oe.order_status = OrderStatus.PAYMENT_FAILED;
            }
            this.sellerRepository.UpdateRange(entries);
            this.sellerRepository.FlushUpdates();
            txCtx.Commit();
        }
    }

    public SellerDashboard QueryDashboard(int sellerId)
    {
        using (var txCtx = this.sellerRepository.BeginTransaction())
        {
            return this.sellerRepository.QueryDashboard(sellerId);
        }
    }

    public void Cleanup()
    {
        this.sellerRepository.Cleanup();
    }

    public void Reset()
    {
        this.sellerRepository.Reset();
    }

}
