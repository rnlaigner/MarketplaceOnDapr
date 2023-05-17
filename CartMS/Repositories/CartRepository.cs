using System;
using Common.Entities;
using Dapr.Client;
using CartMS.Controllers;
using Common.Events;
using System.Net;

namespace CartMS.Repositories
{

    /**
     * I tried as much as possible keep app logic out of here
     * The treatments are more related to concurrrency issues that 
     * could arise within concurrent requests coming from the same customer
     * (although the driver does not allow for that, it is important 
     * that the implementation remains bug-free)
     */
    public class CartRepository : ICartRepository
    {
        public const string StoreName = "statestore";

        private readonly DaprClient daprClient;

        private readonly ILogger<CartRepository> logger;

        public CartRepository(DaprClient daprClient, ILogger<CartRepository> logger)
        {
            this.daprClient = daprClient;
            this.logger = logger;
        }

        public async Task<bool> AddItem(string customerId, CartItem item)
        {
            var cartEntry = await daprClient.GetStateEntryAsync<Cart>(StoreName, customerId);

            if (cartEntry.Value.status == CartStatus.CHECKOUT_SENT)
            {
                return false;
            }

            if (cartEntry.Value.customerId.Equals(""))
            {
                this.logger.LogInformation("Creating cart for customer {0}.", customerId);
                // update field
                cartEntry.Value.customerId = customerId;
                cartEntry.Value.createdAt = DateTime.Now;
                return await this.daprClient.TrySaveStateAsync<Cart>(StoreName,
                    customerId,
                    cartEntry.Value,
                    cartEntry.ETag);
            }

            if (cartEntry.Value.items.ContainsKey(item.ProductId))
            {
                // probably for cases where there are price divergence
                this.logger.LogInformation("Item {0} already added to cart {1}. Item will be overwritten then.", item.ProductId, customerId);
                cartEntry.Value.items[item.ProductId] = item;
            } else {
                this.logger.LogInformation("Item {0} added to cart {1}.", item.ProductId, customerId);
                cartEntry.Value.items.Add(item.ProductId, item);
            }

            return await this.daprClient.TrySaveStateAsync<Cart>(StoreName, customerId, cartEntry.Value, cartEntry.ETag);
        }

        public async Task<Cart> GetCart(string customerId)
        {
            return await this.daprClient.GetStateAsync<Cart>(StoreName, customerId);
        }

        public async Task<bool> Checkout(Cart cart)
        {
            var (_, ETag) = await this.daprClient.GetStateAndETagAsync<Cart>(StoreName, cart.customerId);
            bool res = await this.daprClient.TrySaveStateAsync<Cart>(StoreName,
                    cart.customerId,
                    cart,
                    ETag);
            return res;
        }

        /**
         * Seal is confluent so no need to check for concurrent operation
         */
        public async Task Seal(Cart cart)
        {
            await this.daprClient.SaveStateAsync<Cart>(StoreName,
                    cart.customerId,
                    cart);
        }
    }
}

