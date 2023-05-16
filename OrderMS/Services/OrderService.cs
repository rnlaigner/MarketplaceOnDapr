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

namespace OrderMS.Handlers
{
	public class OrderService
	{

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
            throw new Exception("Not implemented yet!");
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

                order.count_delivered_items += notification.packageInfo.Count();

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
        public async Task<PaymentRequest> ProcessCheckout(ProcessCheckoutRequest checkout)
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

                decimal total_items = total_amount;

                // apply vouchers, but only until total >= 0
                int v_idx = 0;
                decimal[] vouchers = checkout.customerCheckout.Vouchers == null ? emptyArray : checkout.customerCheckout.Vouchers;
                decimal total_incentive = 0;
                while (total_amount > 0 && v_idx < vouchers.Length)
                {
                    if (total_amount - vouchers[v_idx] >= 0)
                    {
                        total_amount -= vouchers[v_idx];
                        total_incentive += vouchers[v_idx];
                    }
                    else
                    {
                        total_amount = 0;
                    }
                }

                OrderModel newOrder = new()
                {
                    customer_id = checkout.customerCheckout.CustomerId,
                    // olist have seller acting in the approval process
                    // here we approve automatically
                    // besides, invoice is a request for payment, so it makes sense to use this status now
                    status = OrderStatus.INVOICED,
                    created_at = System.DateTime.Now,
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
                        seller_id = item.SellerId,
                        unit_price = item.UnitPrice,
                        quantity = item.Quantity,
                        total_items = item.UnitPrice * item.Quantity,
                        total_amount = (item.Quantity * item.FreightValue) + (item.Quantity * item.UnitPrice)
                    };

                    dbContext.OrderItems.Add(oim);

                    orderItems.Add(AsOrderItem(oim));

                    id++;
                }

                // initialize order history
                this.dbContext.OrderHistory.Add(new OrderHistoryModel()
                {
                    order_id = orderTrack.Entity.id,
                    created_at = newOrder.created_at,
                    orderStatus = OrderStatus.INVOICED,
                });

                this.dbContext.SaveChanges();

                PaymentRequest paymentRequest = new PaymentRequest(checkout.customerCheckout, orderTrack.Entity.id, total_amount, orderItems, checkout.instanceId);

                // publish
                await this.daprClient.PublishEventAsync(PUBSUB_NAME, nameof(PaymentRequest), paymentRequest);

                txCtx.Commit();

                return paymentRequest;
            }
		}

        private OrderItem AsOrderItem(OrderItemModel orderItem)
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
                total_amount = orderItem.total_amount
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

    }
}

