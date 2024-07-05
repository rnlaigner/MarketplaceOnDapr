using Common.Events;
using SellerMS.DTO;

namespace SellerMS.Services;

public interface ISellerService
{
    void ProcessInvoiceIssued(InvoiceIssued invoiceIssued);

    void ProcessPaymentConfirmed(PaymentConfirmed paymentConfirmed);
    void ProcessPaymentFailed(PaymentFailed paymentFailed);
        
    void ProcessShipmentNotification(ShipmentNotification shipmentNotification);
    void ProcessDeliveryNotification(DeliveryNotification deliveryNotification);

    SellerDashboard QueryDashboard(int sellerId);

    void Cleanup();
    void Reset();
}

