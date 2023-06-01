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
using OrderMS.Services;

namespace OrderMS.Controllers
{
    [ApiController]
    public class EventHandler
    {
        private const string PUBSUB_NAME = "pubsub";

        private readonly DaprClient daprClient;
        private readonly IOrderService orderService;

        private readonly ILogger<EventHandler> logger;

        public EventHandler(IOrderService eventHandler,
                            DaprClient daprClient,
                            ILogger<EventHandler> logger)
        {
            this.orderService = eventHandler;
            this.daprClient = daprClient;
            this.logger = logger;
        }

        [HttpPost("ProcessCheckout")]
        [Topic(PUBSUB_NAME, nameof(global::Common.Events.StockConfirmed))]
        public async void ProcessCheckout(StockConfirmed checkout)
        {
            this.logger.LogInformation("[ProcessCheckoutRequest] received {0}.", checkout.instanceId);
            await this.orderService.ProcessCheckoutAsync(checkout);
            this.logger.LogInformation("[ProcessCheckoutRequest] processed {0}.", checkout.instanceId);
        }

        [HttpPost("ProcessShipmentNotification")]
        [Topic(PUBSUB_NAME, nameof(ShipmentNotification))]
        public void ProcessShipmentNotification(ShipmentNotification shipmentNotification)
        {
            this.logger.LogInformation("[DeliveryNotification] received {0}.", shipmentNotification.instanceId);
            this.orderService.ProcessShipmentNotification(shipmentNotification);
            this.logger.LogInformation("[ProcessCheckoutRequest] processed {0}.", shipmentNotification.instanceId);
        }

        [HttpPost("ProcessPaymentConfirmed")]
        [Topic(PUBSUB_NAME, nameof(PaymentConfirmed))]
        public void ProcessPaymentConfirmed(PaymentConfirmed paymentConfirmed)
        {
            this.logger.LogInformation("[PaymentConfirmation] received {0}.", paymentConfirmed.instanceId);
            this.orderService.ProcessPaymentConfirmed(paymentConfirmed);
            this.logger.LogInformation("[PaymentConfirmation] processed {0}.", paymentConfirmed.instanceId);
        }

        [HttpPost("ProcessPaymentFailed")]
        [Topic(PUBSUB_NAME, nameof(PaymentFailed))]
        public void ProcessPaymentFailed(PaymentFailed paymentFailed)
        {
            this.logger.LogInformation("[PaymentFailure] received {0}.", paymentFailed.instanceId);
            this.orderService.ProcessPaymentFailed(paymentFailed);
            this.logger.LogInformation("[PaymentFailure] processed {0}.", paymentFailed.instanceId);
        }

    }
}

