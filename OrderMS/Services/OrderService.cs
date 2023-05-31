using System;
using System.Collections.Generic;
using System.Linq;
using Common.Entities;
using Common.Events;
using Microsoft.EntityFrameworkCore;
using OrderMS.Common.Models;
using OrderMS.Infra;
using Dapr.Client;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using OrderMS.Common.Repositories;
using System.Text;
using System.Globalization;
using System.Text.Json.Serialization;
using System.Text.Json;
using OrderMS.Services;

namespace OrderMS.Handlers
{
    public class OrderService : IOrderService
    {

        private static readonly CultureInfo enUS = CultureInfo.CreateSpecificCulture("en-US");
        private static readonly DateTimeFormatInfo dtfi = enUS.DateTimeFormat;
        static OrderService()
        {
            // https://learn.microsoft.com/en-us/dotnet/api/system.globalization.datetimeformatinfo?view=net-7.0
            dtfi.ShortDatePattern = "yyyyMMdd";
        }

        private const string PUBSUB_NAME = "pubsub";

        // TODO change to repository so we can test this component with sqlite
        private readonly OrderDbContext dbContext;
        private readonly IOrderRepository orderRepository;
        private readonly DaprClient daprClient;

        private readonly ILogger<OrderService> logger;

        private static readonly decimal[] emptyArray = Array.Empty<decimal>();

        public OrderService(OrderDbContext dbContext, IOrderRepository orderRepository, DaprClient daprClient, ILogger<OrderService> logger)
        {
            
            this.dbContext = dbContext;
            this.orderRepository = orderRepository;
            this.daprClient = daprClient;
            this.logger = logger;
        }

        public void ProcessShipmentNotification(ShipmentNotification notification)
        {
            using (var txCtx = dbContext.Database.BeginTransaction())
            {
                OrderModel order = orderRepository.GetOrderForUpdate(notification.orderId);
                order.status = OrderStatus.IN_TRANSIT;
                order.updated_at = DateTime.Now;
                this.dbContext.SaveChanges();
                txCtx.Commit();
            }
        }
        /**
         * Providing exactly-once for this case is tricky. The update transaction in shipment can fail in the middle,
         * but some events may have already been generated. In case the update is triggered again, the delivery notifications will be generated again
         * (so we can ensure idempotency here) but if the update never triggers again, the delivery updates generated here will be inconsistent with the shipment.
         */
        public void ProcessDeliveryNotification(DeliveryNotification notification)
        {
            using (var txCtx = dbContext.Database.BeginTransaction())
            {
                
                // check if this event has already been delivered
                OrderHistoryModel orderHistory = this.dbContext.OrderHistory.Where(c => c.instanceId == notification.instanceId).First();
                if(orderHistory is not null)
                {
                    this.logger.LogInformation("[ProcessDeliveryNotification] Event already processed before. InstanceId {0}", notification.instanceId);
                    return;
                }

                DateTime now = DateTime.Now;

                orderHistory = new()
                {
                    order_id = notification.orderId,
                    created_at = now,
                    instanceId = notification.instanceId,
                    packageStatus = PackageStatus.delivered,

                };

                this.dbContext.OrderHistory.Add(orderHistory);

                OrderModel order = orderRepository.GetOrderForUpdate(notification.orderId);

                order.count_delivered_items += 1;

                if (order.count_delivered_items == order.count_items)
                {
                    order.status = OrderStatus.DELIVERED;
                    this.dbContext.Orders.Update(order);
                    OrderHistoryModel orderHistoryStatusUpdate = new()
                    {
                        order_id = notification.orderId,
                        created_at = now,
                        instanceId = notification.instanceId,
                        orderStatus = OrderStatus.DELIVERED

                    };
                    this.dbContext.OrderHistory.Add(orderHistoryStatusUpdate);
                }

                this.dbContext.SaveChanges();
                txCtx.Commit();

            }
            this.logger.LogInformation("[ProcessDeliveryNotification] Event processed. InstanceId {0}", notification.instanceId);
        }

        // for workflow
        public async Task<InvoiceIssued> ProcessCheckout(StockConfirmed checkout)
		{
            // multi-key transaction. to ensure atomicity

            // https://learn.microsoft.com/en-us/ef/ef6/saving/transactions?redirectedfrom=MSDN
            using (var txCtx = dbContext.Database.BeginTransaction())
            {
   
                // calculate total freight_value
                decimal total_freight = 0;
                foreach (var item in checkout.items)
                {
                    total_freight += item.FreightValue;
                }

                decimal total_amount = 0;
                foreach (var item in checkout.items)
                {
                    total_amount += (item.UnitPrice * item.Quantity);
                }

                // total before discounts
                decimal total_items = total_amount;

                // apply vouchers per product, but only until total >= 0 for each item
                Dictionary<long, decimal> totalPerItem = new();
                decimal total_incentive = 0;
                foreach(var item in checkout.items) {
                    decimal total_item = item.UnitPrice * item.Quantity;
                    int v_idx = 0;
                    while (total_item > 0 && v_idx < item.Vouchers.Count())
                    {
                        if (total_item - item.Vouchers[v_idx] >= 0)
                        {
                            total_item -= item.Vouchers[v_idx];
                            total_amount -= item.Vouchers[v_idx];
                            total_incentive += item.Vouchers[v_idx];
                        }
                        else
                        {
                            total_item = 0;
                        }
                    }
                    totalPerItem.Add(item.ProductId, total_item);
                }

                // https://finom.co/en-fr/blog/invoice-number/
                // postresql does not give us sequence ids. it is interesting for the analyst to get
                // a sense of how much orders this customer has made by simply looking at patterns in
                // the invoice. the format <customer_id>-<long_date>-total_orders+1 can be represented like:
                // 50-20220928-001
                // it is inefficient to get count(*) on customer orders. better to have a table like tpc-c does for next_order_id

                var customer_order = this.dbContext.CustomerOrders
                                    .FromSqlRaw(String.Format("SELECT * from customer_orders where customer_id = {0} FOR UPDATE", checkout.customerCheckout.CustomerId))
                                    // .Select(q=>q.next_order_id)
                                    .FirstOrDefault();

                if (customer_order is null)
                {
                    customer_order = new()
                    {
                        customer_id = checkout.customerCheckout.CustomerId,
                        next_order_id = 1
                    };
                    this.dbContext.CustomerOrders.Add(customer_order);
                }
                else
                {
                    customer_order.next_order_id += 1;
                    this.dbContext.CustomerOrders.Update(customer_order);
                }

                var now = System.DateTime.Now;
                StringBuilder stringBuilder = new StringBuilder().Append(checkout.customerCheckout.CustomerId)
                                                                 .Append("-").Append(now.ToString("d", enUS))
                                                                 .Append("-").Append(customer_order.next_order_id);

                OrderModel newOrder = new()
                {
                    customer_id = checkout.customerCheckout.CustomerId,
                    invoice_number = stringBuilder.ToString(),
                    // olist have seller acting in the approval process
                    // here we approve automatically
                    // besides, invoice is a request for payment, so it makes sense to use this status now
                    status = OrderStatus.INVOICED,
                    created_at = now,
                    purchase_date = checkout.createdAt,
                    total_amount = total_amount,
                    total_items = total_items,
                    total_freight = total_freight,
                    total_incentive = total_incentive,
                    total_invoice = total_amount + total_freight,
                    count_items = checkout.items.Count(),
                    instanceId = checkout.instanceId
                };
                var orderTrack = dbContext.Orders.Add(newOrder);

                // save for obtaining the ID generated by postgresql
                this.dbContext.SaveChanges();

                List<OrderItem> orderItems = new(checkout.items.Count);

                int id = 0;
                foreach (var item in checkout.items)
                {

                    OrderItemModel oim = new()
                    {
                        order_id = orderTrack.Entity.id,
                        order_item_id = id,
                        product_id = item.ProductId,
                        product_name = item.Name,
                        product_category = item.Category,
                        seller_id = item.SellerId,
                        unit_price = item.UnitPrice,
                        quantity = item.Quantity,
                        total_items = item.UnitPrice * item.Quantity,
                        total_amount = totalPerItem[item.ProductId],
                        freight_value = item.FreightValue,
                        shipping_limit_date = now.AddDays(3)
                    };

                    dbContext.OrderItems.Add(oim);

                    // vouchers to payment can process
                    orderItems.Add(AsOrderItem(oim, item.Vouchers));

                    id++;
                }

                // initialize order history
                this.dbContext.OrderHistory.Add(new OrderHistoryModel()
                {
                    order_id = orderTrack.Entity.id,
                    created_at = newOrder.created_at,
                    orderStatus = OrderStatus.INVOICED,
                    data = JsonSerializer.Serialize(checkout.customerCheckout)
                });

                this.dbContext.SaveChanges();

                InvoiceIssued invoice = new InvoiceIssued(checkout.customerCheckout, orderTrack.Entity.id,  newOrder.invoice_number,
                    now, total_amount, orderItems, checkout.instanceId);

                // publish
                await this.daprClient.PublishEventAsync(PUBSUB_NAME, nameof(InvoiceIssued), invoice);

                txCtx.Commit();

                return invoice;
            }
		}

        private OrderItem AsOrderItem(OrderItemModel orderItem, decimal[] vouchers)
        {
            return new()
            {
                order_id = orderItem.order_id,
                order_item_id = orderItem.order_item_id,
                product_id = orderItem.product_id,
                product_name = orderItem.product_name,
                seller_id = orderItem.seller_id,
                unit_price = orderItem.unit_price,
                quantity = orderItem.quantity,
                total_items = orderItem.total_items,
                total_amount = orderItem.total_amount,
                shipping_limit_date = orderItem.shipping_limit_date,
                vouchers = vouchers
            };
        }

        private Order AsOrder(OrderModel orderModel)
        {
            return new()
            {
                customer_id = orderModel.customer_id,
                status = orderModel.status.ToString(),
                created_at = orderModel.created_at.ToLongDateString(),
                purchase_date = orderModel.purchase_date.ToLongDateString(),
                total_amount = orderModel.total_amount,
                total_items = orderModel.total_items,
                total_freight = orderModel.total_freight,
                total_incentive = orderModel.total_incentive,
                total_invoice = orderModel.total_invoice,
                count_items = orderModel.count_items,
            }; 
        }

        public void ProcessPaymentConfirmed(PaymentConfirmed paymentConfirmed)
        {
            using (var txCtx = dbContext.Database.BeginTransaction())
            {
                OrderModel order = orderRepository.GetOrderForUpdate(paymentConfirmed.orderId);
                order.status = OrderStatus.PAYMENT_PROCESSED;
                this.dbContext.SaveChanges();
                txCtx.Commit();
            }
        }

        public void ProcessPaymentFailed(PaymentFailed paymentFailed)
        {
            using (var txCtx = dbContext.Database.BeginTransaction())
            {
                OrderModel order = orderRepository.GetOrderForUpdate(paymentFailed.orderId);
                order.status = OrderStatus.PAYMENT_FAILED;
                this.dbContext.SaveChanges();
                txCtx.Commit();
            }
        }

    }
}

