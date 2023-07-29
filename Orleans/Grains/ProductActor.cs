using Common.Entities;
using Common.Events;
using Common.Requests;
using Microsoft.Extensions.Logging;
using Orleans.Interfaces;
using Orleans.Runtime;
using Orleans.Streams;

namespace Orleans.Grains
{
    public class ProductActor : Grain, IProductActor
    {

        private IAsyncStream<ProductUpdate> _producer;

        private readonly IPersistentState<Product> product;
        private readonly ILogger<CartActor> _logger;

        public ProductActor([PersistentState(
            stateName: "product",
            storageName: Infra.Constants.storage)] IPersistentState<Product> state,
            ILogger<CartActor> _logger)
        {
            this.product = state;
            this._logger = _logger;
        }

        public override async Task OnActivateAsync()
        {
            var grainId = this.GetGrainIdentity();
            long primaryKey = this.GetPrimaryKeyLong(out string keyExtension);
            var streamNamespace = string.Format("{0}|{1}", primaryKey, keyExtension);
            await BecomeProducer(streamNamespace);
        }

        public Task BecomeProducer(string streamNamespace)
        {
            _logger.LogInformation("Producer.BecomeProducer");
            IStreamProvider streamProvider = this.GetStreamProvider(Infra.Constants.DefaultStreamProvider);
            IAsyncStream<ProductUpdate> stream = streamProvider.GetStream<ProductUpdate>(
                    Infra.Constants.ProductStreamId, 
                    streamNamespace);
            _producer = stream;
            return Task.CompletedTask;
        }

        public async Task SetProduct(Product product)
        {
            // notify seller
            this.product.State = product;

            ISellerActor sellerActor = this.GrainFactory.GetGrain<ISellerActor>(product.seller_id);
            await Task.WhenAll( sellerActor.IndexProduct(product.product_id),
                                this.product.WriteStateAsync() );
        }

        public async Task DeleteProduct(DeleteProduct productToDelete)
        {
            // delete from stock 
            if(this.product.State != null)
            {
                this.product.State.active = false;
                await this.product.WriteStateAsync();
                // no need to send delete product to cart? sure we should send it too
                // but must send to stock...
                ProductUpdate productUpdate = new()
                {
                    seller_id = productToDelete.sellerId,
                    product_id = productToDelete.productId,
                    price = this.product.State.price,
                    active = false,
                    instanceId = productToDelete.instanceId
                };
                await _producer.OnNextAsync(productUpdate);
            }
            
        }

        public Task<Product> GetProduct()
        {
            return Task.FromResult(this.product.State);
        }

        public async Task UpdatePrice(UpdatePrice updatePrice)
        {
            // no way to know which carts contain the product, so require streaming it
            this.product.State.price = updatePrice.price;
            ProductUpdate productUpdate = new()
            {
                seller_id = updatePrice.sellerId,
                product_id = updatePrice.productId,
                price = this.product.State.price,
                active = true,
                instanceId = updatePrice.instanceId
            };
            await _producer.OnNextAsync(productUpdate);
            await this.product.WriteStateAsync();
        }
    }
}
