using System;
using Common.Entities;
using Common.Events;
using Dapr;
using Dapr.AspNetCore;
using Dapr.Client;
using Microsoft.AspNetCore.Mvc;
using StockMS.Services;

namespace StockMS.Controllers
{
	[ApiController]
	public class EventController : ControllerBase
	{
        private const string PUBSUB_NAME = "pubsub";
        private readonly ILogger<EventController> logger;
        private readonly IStockService stockService;

        public EventController(IStockService stockService,
                               ILogger<EventController> logger)
        {
            this.stockService = stockService;
            this.logger = logger;
        }

        [HttpPost("ReserveStock")]
        [Topic(PUBSUB_NAME, nameof(ReserveStock))]
        public async Task<ActionResult> ProcessReserveStock([FromBody] ReserveStock checkout)
        {
            this.logger.LogInformation("[ReserveStock] received for instanceId {0}", checkout.instanceId);
            await this.stockService.ReserveStockAsync(checkout);
            this.logger.LogInformation("[ReserveStock] completed for instanceId {0}", checkout.instanceId);
            return Ok();
        }

        [HttpPost("ProcessPaymentConfirmed")]
        [Topic(PUBSUB_NAME, nameof(PaymentConfirmed), DeadLetterTopic ="failedConfirmReservation")]
        public ActionResult ProcessPaymentConfirmed([FromBody] PaymentConfirmed paymentConfirmed)
        {
            this.stockService.ConfirmReservation(paymentConfirmed);
            return Ok();
        }

        [HttpPost("failedConfirmReservation")]
        [Topic(PUBSUB_NAME, "failedConfirmReservation")]
        public ActionResult ProcessFailedPaymentConfirmed([FromBody] PaymentConfirmed paymentConfirmed)
        {
            // this.stockService.ConfirmReservation(paymentConfirmed);
            logger.LogWarning("Confirming that the message has been forwarded to the dead letter topic.");
            return Ok();
        }

        [HttpPost("ProcessPaymentFailed")]
        [Topic(PUBSUB_NAME, nameof(PaymentFailed), DeadLetterTopic = "failedCancelReservation")]
        public ActionResult ProcessPaymentFailed([FromBody] PaymentFailed paymentFailed)
        {
            this.stockService.CancelReservation(paymentFailed);
            return Ok();
        }

        [HttpPost("ProductStreaming")]
        [Topic(PUBSUB_NAME, nameof(Product))]
        public ActionResult ProcessProductStream([FromBody] Product product)
        {
            this.stockService.ProcessProductUpdate(product);
            return Ok();
        }

        [HttpPost("BulkProductStreaming")]
        [BulkSubscribe("BulkProductStreaming")]
        [Topic(PUBSUB_NAME, "Products")]
        public ActionResult<BulkSubscribeAppResponse> BulkProcessProductStream([FromBody] BulkSubscribeMessage<BulkMessageModel<Product>> bulkMessages)
        {
            this.stockService.ProcessProductUpdates(bulkMessages.Entries.Select(e => e.Event.Data).ToList());
            List<BulkSubscribeAppResponseEntry>
                responseEntries = bulkMessages.Entries.Select(message => new BulkSubscribeAppResponseEntry(message.EntryId, BulkSubscribeAppResponseStatus.SUCCESS)).ToList();
            return new BulkSubscribeAppResponse(responseEntries);
        }

    }
}

