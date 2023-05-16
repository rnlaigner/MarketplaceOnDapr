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
    public class EventHandler
    {
        private const string PUBSUB_NAME = "pubsub";

        private readonly DaprClient daprClient;
        private readonly OrderService orderService;

        private readonly ILogger<EventHandler> logger;

        public EventHandler(OrderService eventHandler,
                                DaprClient daprClient,
                                ILogger<EventHandler> logger)
        {
            this.orderService = eventHandler;
            this.daprClient = daprClient;
            this.logger = logger;
        }

        [HttpPost("ProcessCheckout")]
        [Topic(PUBSUB_NAME, nameof(ProcessCheckoutRequest))]
        public async void ProcessCheckout(ProcessCheckoutRequest checkout)
        {
            this.logger.LogInformation("[ProcessCheckoutRequest] received {0}.", checkout.instanceId);
            PaymentRequest paymentRequest = await this.orderService.ProcessCheckout(checkout);
            this.logger.LogInformation("[ProcessCheckoutRequest] processed {0}.", checkout.instanceId);
        }

        [HttpPost("ProcessDeliveryNotification")]
        [Topic(PUBSUB_NAME, nameof(DeliveryNotification))]
        public void ProcessDeliveryNotification(DeliveryNotification deliveryNotification)
        {
            this.logger.LogInformation("[DeliveryNotification] received {0}.", deliveryNotification.instanceId);
            this.orderService.ProcessDeliveryNotification(deliveryNotification);
            this.logger.LogInformation("[ProcessCheckoutRequest] processed {0}.", deliveryNotification.instanceId);
        }

        [HttpPost("ProcessShipmentNotification")]
        [Topic(PUBSUB_NAME, nameof(ShipmentNotification))]
        public void ProcessShipmentNotification(ShipmentNotification shipmentNotification)
        {
            this.logger.LogInformation("[DeliveryNotification] received {0}.", shipmentNotification.instanceId);
            this.orderService.ProcessShipmentNotification(shipmentNotification);
            this.logger.LogInformation("[ProcessCheckoutRequest] processed {0}.", shipmentNotification.instanceId);
        }

        [HttpPost("ProcessPaymentConfirmation")]
        [Topic(PUBSUB_NAME, nameof(PaymentConfirmation))]
        public void ProcessPaymentConfirmation(PaymentConfirmation paymentConfirmation)
        {
            this.logger.LogInformation("[PaymentConfirmation] received {0}.", paymentConfirmation.instanceId);
            // this.orderService.ProcessShipmentNotification(shipmentNotification);
            this.logger.LogInformation("[PaymentConfirmation] processed {0}.", paymentConfirmation.instanceId);
        }

        [HttpPost("ProcessPaymentFailure")]
        [Topic(PUBSUB_NAME, nameof(PaymentFailure))]
        public void ProcessPaymentFailure(PaymentFailure paymentFailure)
        {
            this.logger.LogInformation("[PaymentFailure] received {0}.", paymentFailure.instanceId);
            // this.orderService.ProcessShipmentNotification(shipmentNotification);
            this.logger.LogInformation("[PaymentFailure] processed {0}.", paymentFailure.instanceId);
        }

    }
}

