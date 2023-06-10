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
using System.Threading.Tasks;

namespace OrderMS.Controllers
{
    [ApiController]
    public class EventHandler : ControllerBase
    {
        private const string PUBSUB_NAME = "pubsub";

        private readonly IOrderService orderService;

        private readonly ILogger<EventHandler> logger;

        public EventHandler(IOrderService orderService, ILogger<EventHandler> logger)
        {
            this.orderService = orderService;
            this.logger = logger;
        }

        [HttpPost("ProcessCheckout")]
        [Topic(PUBSUB_NAME, nameof(StockConfirmed))]
        public async Task<ActionResult> ProcessCheckout([FromBody] StockConfirmed checkout)
        {
            this.logger.LogInformation("[StockConfirmed] received for customer ID {0}.", checkout.customerCheckout.CustomerId);
            await this.orderService.ProcessCheckout(checkout);
            this.logger.LogInformation("[StockConfirmed] processed for customer ID {0}.", checkout.customerCheckout.CustomerId);
            return Ok();
        }

        [HttpPost("ProcessShipmentNotification")]
        [Topic(PUBSUB_NAME, nameof(ShipmentNotification))]
        public ActionResult ProcessShipmentNotification([FromBody] ShipmentNotification shipmentNotification)
        {
            this.logger.LogInformation("[ShipmentNotification] received for order ID {0}.", shipmentNotification.orderId);
            this.orderService.ProcessShipmentNotification(shipmentNotification);
            this.logger.LogInformation("[ShipmentNotification] processed for order ID {0}.", shipmentNotification.orderId);
            return Ok();
        }

        [HttpPost("ProcessPaymentConfirmed")]
        [Topic(PUBSUB_NAME, nameof(PaymentConfirmed))]
        public ActionResult ProcessPaymentConfirmed([FromBody] PaymentConfirmed paymentConfirmed)
        {
            this.logger.LogInformation("[PaymentConfirmed] received for order ID {0}.", paymentConfirmed.orderId);
            this.orderService.ProcessPaymentConfirmed(paymentConfirmed);
            this.logger.LogInformation("[PaymentConfirmed] processed for order ID {0}.", paymentConfirmed.orderId);
            return Ok();
        }

        [HttpPost("ProcessPaymentFailed")]
        [Topic(PUBSUB_NAME, nameof(PaymentFailed))]
        public ActionResult ProcessPaymentFailed([FromBody] PaymentFailed paymentFailed)
        {
            this.logger.LogInformation("[PaymentFailed] received for order ID {0}.", paymentFailed.orderId);
            this.orderService.ProcessPaymentFailed(paymentFailed);
            this.logger.LogInformation("[PaymentFailed] processed for order ID {0}.", paymentFailed.orderId);
            return Ok();
        }

    }
}

