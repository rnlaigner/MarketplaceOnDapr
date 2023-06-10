using Common.Events;
using Dapr;
using Dapr.Client;
using Microsoft.AspNetCore.Mvc;
using PaymentMS.Services;

namespace PaymentMS.Controllers;

[ApiController]
public class PaymentController : ControllerBase
{

    private const string PUBSUB_NAME = "pubsub";

    private readonly IPaymentService paymentService;

    private readonly ILogger<PaymentController> logger;

    public PaymentController(IPaymentService paymentService, ILogger<PaymentController> logger)
    {
        this.paymentService = paymentService;
        this.logger = logger;
    }

    [HttpPost("ProcessPayment")]
    [Topic(PUBSUB_NAME, nameof(Common.Events.InvoiceIssued))]
    public async Task<ActionResult> ProcessPayment([FromBody] InvoiceIssued invoice)
    {
        this.logger.LogInformation("[InvoiceIssued] received for order ID {0}.", invoice.orderId);
        await this.paymentService.ProcessPayment(invoice);
        return Ok();
    }

}

