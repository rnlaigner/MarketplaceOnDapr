using Common.Entities;
using Common.Events;
using Dapr.Client;
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

        public SellerService(DaprClient daprClient, SellerDbContext sellerDbContext, ISellerRepository sellerRepository)
		{
            this.dbContext = sellerDbContext;
            this.sellerRepository = sellerRepository;
        }

        public Task ProcessShipmentNotification(ShipmentNotification shipmentNotification)
        {
            // create the order if does not exist. if exist, update status. using merge statement
            // https://www.postgresql.org/docs/current/sql-merge.html
            throw new NotImplementedException();
        }

        public async Task ProcessDeliveryNotification(DeliveryNotification deliveryNotification)
        {
            using (var txCtx = dbContext.Database.BeginTransaction())
            {
                
                    dbContext.ShipmentEntries.Add(new()
                    {
                        seller_id = deliveryNotification.sellerId,
                        order_id = deliveryNotification.orderId,
                        package_id = deliveryNotification.packageId,
                        product_id = deliveryNotification.productId,
                        product_name = deliveryNotification.productName,
                        // shipment date comes from shipment notification 
                        // shipment_date = deliveryNotification.ship,
                        delivery_date = deliveryNotification.deliveryDate,
                        status = deliveryNotification.status
                    });
                

                dbContext.SaveChanges();

                await txCtx.CommitAsync();
            }
        }

        /**
         * An order entry per seller is created
         * 
         */
        public Task ProcessNewInvoice(InvoiceIssued invoiceIssued)
        {
            // I have to denormalize. i have to calculate the avergae. everything.
            var sellerGroups = invoiceIssued.items
                                        .GroupBy(x => x.seller_id)
                                        .Select(grp => grp.ToList())
                                        .ToList();

            foreach(var sellerGroup in sellerGroups)
            {
                int count_items = sellerGroup.Count();
                decimal total_amount = sellerGroup.Sum(i => i.total_amount);
                decimal total_freight = sellerGroup.Sum(i => i.freight_value);
                decimal total_incentive = sellerGroup.Sum(i => i.total_amount - i.total_items);
                decimal total_invoice = sellerGroup.Sum(i => i.total_amount + i.freight_value);
                decimal total_items = sellerGroup.Sum(i => i.total_items);

                OrderSeller oevm = new()
                {

                };


                foreach (var items in sellerGroup)
                {
                    ShipmentEntry sevm = new()
                    {

                    };
                }
            }

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

        public Task ProcessStockItem(StockItem stockItem)
        {
            throw new NotImplementedException();
        }

    }
}

