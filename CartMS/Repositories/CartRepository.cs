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

        public async Task<bool> AddItem(long customerId, CartItem item)
        {
            var cartEntry = await daprClient.GetStateEntryAsync<Cart>(StoreName, customerId.ToString());

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
                    customerId.ToString(),
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

            return await this.daprClient.TrySaveStateAsync<Cart>(StoreName, customerId.ToString(), cartEntry.Value, cartEntry.ETag);
        }

        public async Task<Cart> GetCart(long customerId)
        {
            return await this.daprClient.GetStateAsync<Cart>(StoreName, customerId.ToString());
        }

        public async Task<bool> SafeSave(Cart cart)
        {
            var (_, ETag) = await this.daprClient.GetStateAndETagAsync<Cart>(StoreName, cart.customerId.ToString());
            bool res = await this.daprClient.TrySaveStateAsync<Cart>(StoreName,
                    cart.customerId.ToString(),
                    cart,
                    ETag);
            return res;
        }

        public async Task Save(Cart cart)
        {
            await this.daprClient.SaveStateAsync<Cart>(StoreName,
                    cart.customerId.ToString(),
                    cart);
        }
    }
}

