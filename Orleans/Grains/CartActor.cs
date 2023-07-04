using Common.Entities;
using Common.Events;
using Common.Requests;
using Microsoft.Extensions.Logging;
using Orleans.Interfaces;
using Orleans.Runtime;

namespace Orleans.Grains
{

    public class CartActor : Grain, ICartActor
    {
        private readonly IPersistentState<Cart> cart;

        private long customerId;
        private readonly ILogger<CartActor> _logger;

        private Customer customer;

        public CartActor([PersistentState(
            stateName: "cart",
            storageName: "OrleansStorage")] IPersistentState<Cart> state, 
            ILogger<CartActor> _logger)
        {
            this.cart = state;
            this._logger = _logger;
        }

        public override async Task OnActivateAsync()
        {
            this.customerId = this.GetPrimaryKeyLong();
            var custActor = this.GrainFactory.GetGrain<ICustomerActor>(customerId);
            this.customer = await custActor.GetCustomer();
            this._logger.LogWarning("Customer loaded for cart {0}", customerId);
        }

        public async Task AddItem(CartItem item)
        {
            if (item.Quantity <= 0)
            {
                throw new Exception("Item " + item.ProductId + " shows no positive quantity.");
            }

            if (cart.State.status == CartStatus.CHECKOUT_SENT)
            {
                throw new Exception("Cart for customer " + this.customerId + " already sent for checkout.");
            }

            cart.State.items.Add(item);
            await cart.WriteStateAsync();
        }

        public Task<Cart> GetCart()
        {
            this._logger.LogWarning("Cart {0} GET cart request.", this.customerId);
            return Task.FromResult(this.cart.State);
        }

        // customer decided to checkout
        public async Task NotifyCheckout(CustomerCheckout customerCheckout)
        {
            this._logger.LogWarning("Cart {0} received checkout request.", this.customerId);

            if (this.customerId != customerCheckout.CustomerId)
                throw new Exception("Cart " + this.customerId + " does not correspond to customr ID received: " + customerCheckout.CustomerId);

            if (this.cart.State.status == CartStatus.CHECKOUT_SENT)
                throw new Exception("Cannot checkout a cart " + customerId + " that has a checkout in progress.");

            if (this.cart.State.items.Count == 0)
                throw new Exception("Cart " + this.customerId + " is empty.");

            this.cart.State.status = CartStatus.CHECKOUT_SENT;
            ReserveStock checkout = new ReserveStock(DateTime.Now, customerCheckout, cart.State.items, customerCheckout.instanceId);

            IOrderActor orderActor = this.GrainFactory.GetGrain<IOrderActor>(this.customerId);
          
            _ = orderActor.Checkout(checkout);
            
            await Seal();
        }

        public async Task Seal()
        {
            if (this.cart.State.status == CartStatus.CHECKOUT_SENT)
            {
                this.cart.State.status = CartStatus.OPEN;
                this.cart.State.items.Clear();
                await cart.WriteStateAsync();
            }
            else
            {
                throw new Exception("Cannot seal a cart that has not been checked out");
            }
        }

    }

}
