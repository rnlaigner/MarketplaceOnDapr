using System.Linq;
using System.Text.Json;
using Common.Entities;
using Common.Events;
using Dapr;
using Dapr.AspNetCore;
using Dapr.Client;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SellerMS.Repositories;
using SellerMS.Services;

namespace CartMS.Controllers;

[ApiController]
public class EventController : ControllerBase
{
    private const string PUBSUB_NAME = "pubsub";

    private readonly ILogger<EventController> logger;
    private readonly ISellerService sellerService;

    public EventController(ISellerService sellerService, 
                           ILogger<EventController> logger)
    {
        this.sellerService = sellerService;
        this.logger = logger;
    }

    [HttpPost("ProcessPaymentConfirmed")]
    [Topic(PUBSUB_NAME, nameof(PaymentConfirmed))]
    public void ProcessPaymentConfirmed([FromBody] PaymentConfirmed paymentConfirmed)
    {

    }

    [HttpPost("ProcessPaymentFailed")]
    [Topic(PUBSUB_NAME, nameof(PaymentFailed))]
    public void ProcessPaymentFailed([FromBody] PaymentFailed paymentFailed)
    {

    }

    [HttpPost("ProcessNewInvoice")]
    [Topic(PUBSUB_NAME, nameof(InvoiceIssued))]
    public async void ProcessNewInvoice([FromBody] InvoiceIssued paymentRequest)
    {

    }

    [HttpPost("ProcessProductUpdate")]
    [Topic(PUBSUB_NAME, nameof(Product))]
    public void ProcessProductUpdate([FromBody] Product product)
    {

    }

    [HttpPost("ProcessStockItem")]
    [Topic(PUBSUB_NAME, nameof(StockItem))]
    public void ProcessStockItem([FromBody] StockItem stockItem)
    {

    }

    [HttpPost("ProcessShipmentNotification")]
    [Topic(PUBSUB_NAME, nameof(ShipmentNotification))]
    public void ProcessShipmentNotification([FromBody] ShipmentNotification shipmentNotification)
    {

    }

    [HttpPost("ProcessShipmentNotification")]
    [Topic(PUBSUB_NAME, nameof(DeliveryNotification))]
    public void ProcessDeliveryNotification([FromBody] DeliveryNotification deliveryNotification)
    {

    }

    // TODO customer ms. also relational. notifications.store behavior of customer over time. customer dashboard?

}

