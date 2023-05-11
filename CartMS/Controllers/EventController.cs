using System;
using System.Net;
using CartMS.Repositories;
using Common.Entities;
using Common.Events;
using Dapr;
using Dapr.Client;
using Microsoft.AspNetCore.Mvc;

namespace CartMS.Controllers;

[ApiController]
public class EventController : ControllerBase
{
    private const string PUBSUB_NAME = "pubsub";

    private readonly DaprClient daprClient;
    private readonly ICartRepository cartRepository;

    private readonly ILogger<EventController> logger;
    
    public EventController(DaprClient daprClient, ICartRepository cartRepository, ILogger<EventController> logger)
    {
        this.daprClient = daprClient;
        this.cartRepository = cartRepository;
        this.logger = logger;
    }

    [HttpPost("NotifyCheckout")]
    [Topic(PUBSUB_NAME, nameof(CustomerCheckout))]
    public async void NotifyCheckout(CustomerCheckout customerCheckout)
    {

        Cart cart = await this.cartRepository.GetCart(customerCheckout.CustomerId);
        if (cart.status == CartStatus.CHECKOUT_SENT)
        {
            this.logger.LogWarning("Customer {0} cart has already been submitted to checkout", customerCheckout.CustomerId);
            return;
        }
        cart.status = CartStatus.CHECKOUT_SENT;
        bool res = await this.cartRepository.Checkout(cart);
        if (!res)
        {
            this.logger.LogWarning("Customer {0} cart has already been submitted to checkout", customerCheckout.CustomerId);
            return;
        }

        // CancellationTokenSource source = new CancellationTokenSource();
        // CancellationToken cancellationToken = source.Token;

        ReserveStockRequest checkout = new ReserveStockRequest(DateTime.Now, customerCheckout, cart.items.Select(c=>c.Value).ToList() );

        await this.daprClient.PublishEventAsync(PUBSUB_NAME, "ReserveInventory", checkout); // , cancellationToken);

    }


}

