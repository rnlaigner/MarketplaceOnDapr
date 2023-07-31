using Common.Entities;
using Common.Events;
using SellerMS.DTO;

namespace SellerMS.Services
{
    public interface ISellerService
    {
        void ProcessDeliveryNotification(DeliveryNotification deliveryNotification);
        void ProcessNewInvoice(InvoiceIssued invoiceIssued);
        void ProcessPaymentConfirmed(PaymentConfirmed paymentConfirmed);
        void ProcessPaymentFailed(PaymentFailed paymentFailed);
        void ProcessProductUpdate(Product product);
        void ProcessShipmentNotification(ShipmentNotification shipmentNotification);
        void ProcessStockItem(StockItem stockItem);

        SellerDashboard QueryDashboard(int sellerId);

        void Cleanup();
        void Reset();
    }
}

