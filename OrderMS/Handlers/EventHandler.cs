using Common.Events;
using Dapr;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
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
            await this.orderService.ProcessCheckout(checkout);
            return Ok();
        }

        [HttpPost("ProcessShipmentNotification")]
        [Topic(PUBSUB_NAME, nameof(ShipmentNotification))]
        public ActionResult ProcessShipmentNotification([FromBody] ShipmentNotification shipmentNotification)
        {
            this.orderService.ProcessShipmentNotification(shipmentNotification);
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

    }
}

