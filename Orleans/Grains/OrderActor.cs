using Common.Entities;
using Common.Events;
using Orleans.Concurrency;
using Orleans.Interfaces;
using Orleans.Runtime;

namespace Orleans.Grains
{
    [Reentrant]
    public class OrderActor : Grain, IOrderActor
    {

        private readonly IPersistentState<Dictionary<long, Order>> orderState;
        private readonly IPersistentState<Dictionary<long, OrderItem>> orderItemState;
        private long customerId;

        private long nextOrderId;

        public OrderActor(
            [PersistentState(stateName: "order", storageName: "OrleansStorage")]
            IPersistentState<Dictionary<long, Order>> orderState,
            [PersistentState(stateName: "orderItem", storageName: "OrleansStorage")]
            IPersistentState<Dictionary<long, OrderItem>> orderItemState) 
        { 
            this.orderState = orderState;
            this.orderItemState = orderItemState;
            this.nextOrderId = 1;
        }

        public override async Task OnActivateAsync()
        {
            this.customerId = this.GetPrimaryKeyLong();
            await base.OnActivateAsync();
        }


        public async Task Checkout(ReserveStock reserveStock)
        {
            var now = DateTime.Now;

            // coordinate with all IStock
            List<Task<ItemStatus>> statusResp = new(reserveStock.items.Count());

            foreach (var item in reserveStock.items)
            {
                var stockActor = GrainFactory.GetGrain<IStockActor>(item.SellerId, item.ProductId.ToString(), null);
                statusResp.Add(stockActor.AttemptReservation(item.Quantity));
            }

            await Task.WhenAll(statusResp);
            int idx = 0;
            var itemsToCheckout = new List<CartItem>(reserveStock.items.Count());
            List<Task> stockTasks= new List<Task>();
            foreach (var item in reserveStock.items)
            {
                if (statusResp[idx].Result == ItemStatus.IN_STOCK)
                {
                    itemsToCheckout.Add(item);
                    var stockActor = GrainFactory.GetGrain<IStockActor>(item.SellerId, item.ProductId.ToString(), null);
                    stockTasks.Add(stockActor.ConfirmReservation(item.Quantity));
                }
            }

            await Task.WhenAll(stockTasks);



            // calculate total freight_value
            decimal total_freight = 0;
            foreach (var item in itemsToCheckout)
            {
                total_freight += item.FreightValue;
            }

            decimal total_amount = 0;
            foreach (var item in itemsToCheckout)
            {
                total_amount += (item.UnitPrice * item.Quantity);
            }

            decimal total_items = total_amount;

            Dictionary<long, decimal> totalPerItem = new();
            decimal total_incentive = 0;
            foreach (var item in itemsToCheckout)
            {
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

            Order newOrder = new()
            {
                id = nextOrderId,
                customer_id = this.customerId,
                status = OrderStatus.INVOICED,
                created_at = System.DateTime.Now,
                purchase_date = reserveStock.timestamp,
                total_amount = total_amount,
                total_items = total_items,
                total_freight = total_freight,
                total_incentive = total_incentive,
                total_invoice = total_amount + total_freight,
                count_items = itemsToCheckout.Count(),

            };

            nextOrderId++;

            return;
        }
    }
}
