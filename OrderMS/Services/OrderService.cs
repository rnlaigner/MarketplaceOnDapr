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
using Common.Idempotency;

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

        public void ProcessShipmentNotification(ShipmentNotification shipmentNotification)
        {
            using (var txCtx = dbContext.Database.BeginTransaction())
            {
                OrderModel order = orderRepository.GetOrderForUpdate(shipmentNotification.orderId);

                DateTime now = DateTime.Now;

                OrderStatus orderStatus = OrderStatus.READY_FOR_SHIPMENT;
                if (shipmentNotification.status == ShipmentStatus.delivery_in_progress) orderStatus = OrderStatus.IN_TRANSIT;
                if (shipmentNotification.status == ShipmentStatus.concluded) orderStatus = OrderStatus.DELIVERED;

                OrderHistoryModel orderHistory = new()
                {
                    order_id = shipmentNotification.orderId,
                    created_at = now,
                    status = orderStatus
                };

                order.status = orderStatus;
                order.updated_at = now;

                if(order.status == OrderStatus.DELIVERED)
                {
                    order.delivered_customer_date = shipmentNotification.eventDate;
                }

                dbContext.Orders.Update(order);
                dbContext.OrderHistory.Add(orderHistory);

                this.dbContext.SaveChanges();
                txCtx.Commit();
            }
        }

        /*
         * Require idempotent computation, otherwise orders will get duplicated
         * (although with different IDs due to PostgreSQL ID generation)
         */
        public async Task ProcessCheckoutAsync(StockConfirmed checkout)
		{
            // multi-key transaction. to ensure atomicity

            // https://learn.microsoft.com/en-us/ef/ef6/saving/transactions?redirectedfrom=MSDN
            using (var txCtx = dbContext.Database.BeginTransaction())
            {
                var now = System.DateTime.Now;

                var tracking = dbContext.TransactionTracking.Find(checkout.instanceId);
                if(tracking is not null)
                {
                    logger.LogWarning("[ProcessCheckout] Idempotence check has found instance ID {0} created at {1}. Ignoring event StockConfirmed.", checkout.instanceId, tracking.createdAt);
                    return;
                }

                tracking = new TransactionTrackingModel(checkout.instanceId, now);
                dbContext.TransactionTracking.Add(tracking);
   
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
                // https://www.amazon.com/gp/help/customer/display.html?nodeId=G9R2MLD3EX557D77
                Dictionary<long, decimal> totalPerItem = new();
                decimal total_incentive = 0;
                foreach(var item in checkout.items) {
                    decimal total_item = item.UnitPrice * item.Quantity;

                    decimal sumVouchers = item.Vouchers.Sum();

                    if (total_item - sumVouchers > 0)
                    {
                        total_amount -= sumVouchers;
                        total_incentive += sumVouchers;
                        total_item -= sumVouchers;
                    }
                    else
                    {
                        total_amount -= total_item;
                        total_incentive += total_item;
                        total_item = 0;
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
                };
                var orderPersisted = dbContext.Orders.Add(newOrder);

                // save for obtaining the ID generated by postgresql
                this.dbContext.SaveChanges();

                List<OrderItem> orderItems = new(checkout.items.Count);

                int id = 0;
                foreach (var item in checkout.items)
                {

                    OrderItemModel oim = new()
                    {
                        order_id = orderPersisted.Entity.id,
                        order_item_id = id,
                        product_id = item.ProductId,
                        product_name = item.ProductName,
                        seller_id = item.SellerId,
                        unit_price = item.UnitPrice,
                        quantity = item.Quantity,
                        total_items = item.UnitPrice * item.Quantity,
                        total_amount = totalPerItem[item.ProductId],
                        freight_value = item.FreightValue,
                        shipping_limit_date = now.AddDays(3)
                    };

                    dbContext.OrderItems.Add(oim);

                    // vouchers so payment can process
                    orderItems.Add(AsOrderItem(oim, item.Vouchers));

                    id++;
                }

                // initialize order history
                this.dbContext.OrderHistory.Add(new OrderHistoryModel()
                {
                    order_id = orderPersisted.Entity.id,
                    created_at = newOrder.created_at,
                    status = OrderStatus.INVOICED,
                    // data = JsonSerializer.Serialize(checkout.customerCheckout)
                });

                this.dbContext.SaveChanges();

                InvoiceIssued invoice = new InvoiceIssued(checkout.customerCheckout, orderPersisted.Entity.id,  newOrder.invoice_number,
                    now, total_amount, orderItems, checkout.instanceId);

                // publish
                await this.daprClient.PublishEventAsync(PUBSUB_NAME, nameof(InvoiceIssued), invoice);

                txCtx.Commit();
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
                status = orderModel.status,
                created_at = orderModel.created_at,
                purchase_date = orderModel.purchase_date,
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
            var now = DateTime.Now;
            using (var txCtx = dbContext.Database.BeginTransaction())
            {
                OrderModel order = orderRepository.GetOrderForUpdate(paymentConfirmed.orderId);
                order.status = OrderStatus.PAYMENT_PROCESSED;
                order.payment_date = paymentConfirmed.date;
                order.updated_at = now;

                dbContext.Orders.Update(order);

                dbContext.OrderHistory.Add(new OrderHistoryModel()
                {
                    order_id = paymentConfirmed.orderId,
                    created_at = now,
                    status = OrderStatus.PAYMENT_PROCESSED,
                });

                dbContext.SaveChanges();
                txCtx.Commit();
            }
        }

        public void ProcessPaymentFailed(PaymentFailed paymentFailed)
        {
            var now = DateTime.Now;
            using (var txCtx = dbContext.Database.BeginTransaction())
            {
                OrderModel order = orderRepository.GetOrderForUpdate(paymentFailed.orderId);
                order.status = OrderStatus.PAYMENT_FAILED;
                order.updated_at = now;

                dbContext.Orders.Update(order);

                dbContext.OrderHistory.Add(new OrderHistoryModel()
                {
                    order_id = paymentFailed.orderId,
                    created_at = now,
                    status = OrderStatus.PAYMENT_FAILED,
                });

                dbContext.SaveChanges();
                txCtx.Commit();
            }
        }

        InvoiceIssued IOrderService.ProcessCheckout(StockConfirmed checkout)
        {
            throw new NotImplementedException();
        }

        public Task ProcessCheckout(StockConfirmed checkout)
        {
            throw new NotImplementedException();
        }
    }
}

