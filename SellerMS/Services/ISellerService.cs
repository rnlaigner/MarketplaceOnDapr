using System;
using Common.Entities;
using Common.Events;

namespace SellerMS.Services
{
    public interface ISellerService
    {
        Task ProcessDeliveryNotification(DeliveryNotification deliveryNotification);
        Task ProcessNewInvoice(InvoiceIssued invoiceIssued);
        Task ProcessPaymentConfirmed(PaymentConfirmed paymentConfirmed);
        Task ProcessPaymentFailed(PaymentFailed paymentFailed);
        Task ProcessProductUpdate(Product product);
        Task ProcessShipmentNotification(ShipmentNotification shipmentNotification);
        Task ProcessStockItem(StockItem stockItem);
    }
}

