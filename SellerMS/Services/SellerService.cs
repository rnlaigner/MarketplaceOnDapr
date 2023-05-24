using Common.Entities;
using Common.Events;
using Dapr.Client;
using SellerMS.Infra;
using SellerMS.Repositories;

namespace SellerMS.Services
{
	public class SellerService : ISellerService
	{

        private readonly DaprClient daprClient;
        private readonly SellerDbContext dbContext;
        private readonly ISellerRepository sellerRepository;

        public SellerService(DaprClient daprClient, SellerDbContext sellerDbContext, ISellerRepository sellerRepository)
		{
            this.dbContext = sellerDbContext;
            this.sellerRepository = sellerRepository;

        }

        public Task ProcessDeliveryNotification(DeliveryNotification deliveryNotification)
        {
            throw new NotImplementedException();
        }

        public Task ProcessNewInvoice(InvoiceIssued invoiceIssued)
        {
            // I have to denormalize. i have to calculate the avergae. everything.
            var exec = dbContext.Database.CreateExecutionStrategy();
            // dbContext.OrderEntries.find
            //dbContext.OrderEntries.
            //using ()

            throw new NotImplementedException();
        }

        public Task ProcessPaymentConfirmed(PaymentConfirmed paymentConfirmed)
        {
            throw new NotImplementedException();
        }

        public Task ProcessPaymentFailed(PaymentFailed paymentFailed)
        {
            throw new NotImplementedException();
        }

        public Task ProcessProductUpdate(Product product)
        {
            throw new NotImplementedException();
        }

        public Task ProcessShipmentNotification(ShipmentNotification shipmentNotification)
        {
            throw new NotImplementedException();
        }

        public Task ProcessStockItem(StockItem stockItem)
        {
            throw new NotImplementedException();
        }
    }
}

