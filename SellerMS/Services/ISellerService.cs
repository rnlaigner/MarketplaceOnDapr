using System;
using Common.Entities;
using Common.Events;
using SellerMS.DTO;
using SellerMS.Models;

namespace SellerMS.Services
{
    public interface ISellerService
    {
        void AddSeller(Seller seller);
        Seller GetSeller(long id);

        Task ProcessDeliveryNotification(DeliveryNotification deliveryNotification);
        Task ProcessNewInvoice(InvoiceIssued invoiceIssued);
        Task ProcessPaymentConfirmed(PaymentConfirmed paymentConfirmed);
        Task ProcessPaymentFailed(PaymentFailed paymentFailed);
        Task ProcessProductUpdate(Product product);
        Task ProcessShipmentNotification(ShipmentNotification shipmentNotification);
        Task ProcessStockItem(StockItem stockItem);

        SellerDashboard QueryDashboard(long sellerId);
    }
}

