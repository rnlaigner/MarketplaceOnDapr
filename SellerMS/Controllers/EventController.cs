using Common.Entities;
using Common.Events;
using Dapr;
using Microsoft.AspNetCore.Mvc;
using SellerMS.Services;

namespace SellerMS.Controllers;

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
    public async void ProcessPaymentConfirmed([FromBody] PaymentConfirmed paymentConfirmed)
    {
        await this.sellerService.ProcessPaymentConfirmed(paymentConfirmed);
    }

    [HttpPost("ProcessPaymentFailed")]
    [Topic(PUBSUB_NAME, nameof(PaymentFailed))]
    public async void ProcessPaymentFailed([FromBody] PaymentFailed paymentFailed)
    {
        await this.sellerService.ProcessPaymentFailed(paymentFailed);
    }

    [HttpPost("ProcessNewInvoice")]
    [Topic(PUBSUB_NAME, nameof(InvoiceIssued))]
    public async void ProcessNewInvoice([FromBody] InvoiceIssued invoiceIssued)
    {
        await this.sellerService.ProcessNewInvoice(invoiceIssued);
    }

    [HttpPost("ProcessProductUpdate")]
    [Topic(PUBSUB_NAME, nameof(Product))]
    public async void ProcessProductUpdate([FromBody] Product product)
    {
        await this.sellerService.ProcessProductUpdate(product);
    }

    [HttpPost("ProcessStockItem")]
    [Topic(PUBSUB_NAME, nameof(StockItem))]
    public async void ProcessStockItem([FromBody] StockItem stockItem)
    {
        await this.sellerService.ProcessStockItem(stockItem);
    }

    [HttpPost("ProcessShipmentNotification")]
    [Topic(PUBSUB_NAME, nameof(ShipmentNotification))]
    public async void ProcessShipmentNotification([FromBody] ShipmentNotification shipmentNotification)
    {
        await this.sellerService.ProcessShipmentNotification(shipmentNotification);
    }

    [HttpPost("ProcessShipmentNotification")]
    [Topic(PUBSUB_NAME, nameof(DeliveryNotification))]
    public async void ProcessDeliveryNotification([FromBody] DeliveryNotification deliveryNotification)
    {
        await this.sellerService.ProcessDeliveryNotification(deliveryNotification);
    }

}

