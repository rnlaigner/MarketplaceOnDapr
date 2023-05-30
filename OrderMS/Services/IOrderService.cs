using System;
using System.Threading.Tasks;
using Common.Events;

namespace OrderMS.Services
{
	public interface IOrderService
	{
        public void ProcessShipmentNotification(ShipmentNotification notification);

        public void ProcessDeliveryNotification(DeliveryNotification notification);

        public Task<InvoiceIssued> ProcessCheckout(StockConfirmed checkout);


    }
}

