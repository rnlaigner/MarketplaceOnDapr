using Common.Entities;
using Common.Events;
using Common.Requests;
using Microsoft.Extensions.Logging;
using Orleans.Interfaces;
using Orleans.Runtime;
using Orleans.Streams;

namespace Orleans.Grains
{

    public class CartActor : Grain, ICartActor
    {

        private List<StreamSubscriptionHandle<ProductUpdate>> consumerHandles;

        private readonly IPersistentState<Cart> cart;

        private long customerId;
        private readonly ILogger<CartActor> _logger;

        private readonly Dictionary<(long, long), ProductUpdate> cachedProducts;

        public CartActor([PersistentState(
            stateName: "cart",
            storageName: Infra.Constants.storage)] IPersistentState<Cart> state, 
            ILogger<CartActor> _logger)
        {
            this.cart = state;
            this._logger = _logger;
            this.consumerHandles = new();
            this.cachedProducts = new();
        }

        public override async Task OnActivateAsync()
        {
            this.customerId = this.GetPrimaryKeyLong();
            await base.OnActivateAsync();
        }

        public async Task BecomeConsumer(string streamNamespace)
        {
            this._logger.LogInformation("BecomeConsumer");
            IStreamProvider streamProvider = this.GetStreamProvider(Infra.Constants.DefaultStreamProvider);
            IAsyncStream<ProductUpdate> _consumer = streamProvider.GetStream<ProductUpdate>( Infra.Constants.ProductStreamId, streamNamespace);
            this.consumerHandles.Add( await _consumer.SubscribeAsync(UpdateProductAsync) );
        }

        public async Task StopConsuming()
        {
            this._logger.LogInformation("StopConsuming");
            if (this.consumerHandles.Count() > 0)
            {
                foreach(var handle in consumerHandles) { await handle.UnsubscribeAsync(); }
            }
            this.consumerHandles.Clear();
        }

        private Task UpdateProductAsync(ProductUpdate product, StreamSequenceToken token)
        {
            this.cachedProducts.Add((product.seller_id, product.product_id), product);
            return Task.CompletedTask;
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

            this.cart.State.items.Add(item);
            await this.cart.WriteStateAsync();
            await BecomeConsumer(string.Format("{0}|{1}", item.SellerId, item.ProductId));
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

            // TODO check if price divergence
            // var cartItem = cart.State.items.First(i => i.SellerId == product.seller_id && i.ProductId == product.product_id);

            ReserveStock checkout = new ReserveStock(DateTime.Now, customerCheckout, cart.State.items, customerCheckout.instanceId);

            IOrderActor orderActor = this.GrainFactory.GetGrain<IOrderActor>(this.customerId);
          
            _ = orderActor.Checkout(checkout);
            
            await Seal();
        }

        public async Task Seal()
        {
            this.cart.State.items.Clear();
            await this.cart.WriteStateAsync();
            await this.StopConsuming();
        }

    }

}
