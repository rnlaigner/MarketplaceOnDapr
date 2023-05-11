using System;
using System.Net;
using OrderMS.Repositories;
using Common.Entities;
using Common.Events;
using Dapr;
using Dapr.Client;
using Microsoft.AspNetCore.Mvc;
using OrderMS.Common.Repositories;
using Microsoft.Extensions.Logging;
using OrderMS.Handlers;

namespace OrderMS.Controllers
{
    [ApiController]
    public class EventController
    {
        private const string PUBSUB_NAME = "pubsub";

        private readonly DaprClient daprClient;
        private readonly OrderEventHandler eventHandler;

        private readonly ILogger<EventController> logger;

        public EventController(OrderEventHandler eventHandler,
                                DaprClient daprClient,
                                ILogger<EventController> logger)
        {
            this.eventHandler = eventHandler;
            this.daprClient = daprClient;
            this.logger = logger;
        }

        [HttpPost("ProcessCheckout")]
        [Topic(PUBSUB_NAME, nameof(ProcessCheckoutRequest))]
        public async void ProcessCheckout(ProcessCheckoutRequest checkout)
        {
            this.logger.LogInformation("[ProcessCheckoutRequest] received {0}.", checkout.instanceId);
            Invoice invoice = this.eventHandler.ProcessCheckout(checkout);
            await this.daprClient.PublishEventAsync(PUBSUB_NAME, "CheckoutResult", invoice);
            this.logger.LogInformation("[ProcessCheckoutRequest] processed {0}.", checkout.instanceId);
        }

    }
}

