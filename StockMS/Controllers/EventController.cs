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

        private readonly DaprClient daprClient;

        private readonly ILogger<EventController> logger;
        private readonly IStockService stockService;

        public EventController(DaprClient daprClient,
                                IStockService stockService,
                                
                                ILogger<EventController> logger)
        {
            this.daprClient = daprClient;
            this.logger = logger;
            this.stockService = stockService;
        }

        [HttpPost("ReserveStock")]
        [Topic(PUBSUB_NAME, nameof(ReserveStock))]
        public async Task ReserveStock([FromBody] ReserveStock checkout)
        {
            this.logger.LogInformation("[ReserveStock] received for instanceId {0}", checkout.instanceId);
            await this.stockService.ReserveStockAsync(checkout);
            this.logger.LogInformation("[ReserveStock] completed for instanceId {0}", checkout.instanceId);
        }

        [HttpPost("ProcessPaymentConfirmed")]
        [Topic(PUBSUB_NAME, nameof(PaymentConfirmed))]
        public void ProcessPaymentConfirmed([FromBody] PaymentConfirmed paymentConfirmed)
        {
            this.stockService.ConfirmReservation(paymentConfirmed);
        }

        [HttpPost("ProcessPaymentFailed")]
        [Topic(PUBSUB_NAME, nameof(PaymentFailed))]
        public void ProcessPaymentFailed([FromBody] PaymentFailed paymentFailed)
        {
            this.stockService.CancelReservation(paymentFailed);
        }

        [HttpPost("ProductStreaming")]
        [Topic(PUBSUB_NAME, nameof(Product))]
        public ActionResult ProcessProductStream([FromBody] Product product)
        {
            this.stockService.ProcessProductUpdate(product);
            return Ok();
            // this.logger.LogInformation("[UpdatePrice] completed for instanceId {0}", priceUpdate.instanceId);
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

