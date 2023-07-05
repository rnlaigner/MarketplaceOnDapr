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

    public EventController(ISellerService sellerService, ILogger<EventController> logger)
    {
        this.sellerService = sellerService;
        this.logger = logger;
    }

    [HttpPost("ProcessPaymentConfirmed")]
    [Topic(PUBSUB_NAME, nameof(PaymentConfirmed))]
    public ActionResult ProcessPaymentConfirmed([FromBody] PaymentConfirmed paymentConfirmed)
    {
        this.logger.LogInformation("[PaymentConfirmed] received for order ID {0}.", paymentConfirmed.orderId);
        this.sellerService.ProcessPaymentConfirmed(paymentConfirmed);
        return Ok();
    }

    [HttpPost("ProcessPaymentFailed")]
    [Topic(PUBSUB_NAME, nameof(PaymentFailed))]
    public ActionResult ProcessPaymentFailed([FromBody] PaymentFailed paymentFailed)
    {
        this.logger.LogInformation("[PaymentFailed] received for order ID {0}.", paymentFailed.orderId);
        this.sellerService.ProcessPaymentFailed(paymentFailed);
        return Ok();
    }

    [HttpPost("ProcessNewInvoice")]
    [Topic(PUBSUB_NAME, nameof(InvoiceIssued))]
    public ActionResult ProcessNewInvoice([FromBody] InvoiceIssued invoiceIssued)
    {
        this.logger.LogInformation("[InvoiceIssued] received for order ID {0}.", invoiceIssued.orderId);
        this.sellerService.ProcessNewInvoice(invoiceIssued);
        return Ok();
    }

    //[HttpPost("ProcessProductUpdate")]
    //[Topic(PUBSUB_NAME, nameof(Product))]
    //public void ProcessProductUpdate([FromBody] Product product)
    //{
    //    this.sellerService.ProcessProductUpdate(product);
    //}

    //[HttpPost("ProcessStockItem")]
    //[Topic(PUBSUB_NAME, nameof(StockItem))]
    //public ActionResult ProcessStockItem([FromBody] StockItem stockItem)
    //{
    //    this.logger.LogInformation("[StockItem] received for item ID {0}.", stockItem.product_id);
    //    this.sellerService.ProcessStockItem(stockItem);
    //    return Ok();
    //}

    [HttpPost("ProcessShipmentNotification")]
    [Topic(PUBSUB_NAME, nameof(ShipmentNotification))]
    public ActionResult ProcessShipmentNotification([FromBody] ShipmentNotification shipmentNotification)
    {
        this.logger.LogInformation("[ShipmentNotification] received for order ID {0}.", shipmentNotification.orderId);
        this.sellerService.ProcessShipmentNotification(shipmentNotification);
        return Ok();
    }

    [HttpPost("ProcessDeliveryNotification")]
    [Topic(PUBSUB_NAME, nameof(DeliveryNotification))]
    public ActionResult ProcessDeliveryNotification([FromBody] DeliveryNotification deliveryNotification)
    {
        this.logger.LogInformation("[DeliveryNotification] received for order ID {0}.", deliveryNotification.orderId);
        this.sellerService.ProcessDeliveryNotification(deliveryNotification);
        return Ok();
    }

}