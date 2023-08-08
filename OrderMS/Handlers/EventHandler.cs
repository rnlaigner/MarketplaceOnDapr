using Common.Events;
using Dapr;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OrderMS.Services;
using System;
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

        [HttpPost("ProcessStockConfirmed")]
        [Topic(PUBSUB_NAME, nameof(StockConfirmed))]
        public async Task<ActionResult> ProcessStockConfirmed([FromBody] StockConfirmed stockConfirmed)
        {
            try
            {
                await this.orderService.ProcessStockConfirmed(stockConfirmed);
            }
            catch (Exception)
            {
                await this.orderService.ProcessPoisonStockConfirmed(stockConfirmed);
            }
            return Ok();
        }

        [HttpPost("ProcessPaymentConfirmed")]
        [Topic(PUBSUB_NAME, nameof(PaymentConfirmed))]
        public ActionResult ProcessPaymentConfirmed([FromBody] PaymentConfirmed paymentConfirmed)
        {
            this.orderService.ProcessPaymentConfirmed(paymentConfirmed);
            return Ok();
        }

        [HttpPost("ProcessPaymentFailed")]
        [Topic(PUBSUB_NAME, nameof(PaymentFailed))]
        public ActionResult ProcessPaymentFailed([FromBody] PaymentFailed paymentFailed)
        {
            this.orderService.ProcessPaymentFailed(paymentFailed);
            return Ok();
        }

        [HttpPost("ProcessShipmentNotification")]
        [Topic(PUBSUB_NAME, nameof(ShipmentNotification))]
        public ActionResult ProcessShipmentNotification([FromBody] ShipmentNotification shipmentNotification)
        {
            this.orderService.ProcessShipmentNotification(shipmentNotification);
            return Ok();
        }

    }
}

