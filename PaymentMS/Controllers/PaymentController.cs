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
    [Topic(PUBSUB_NAME, nameof(Common.Events.InvoiceIssued), "poisonMessages", false)]
    public async Task ProcessPayment(InvoiceIssued paymentRequest)
    {
        this.logger.LogInformation("[ProcessPayment] received: {0}.", paymentRequest.instanceId);
        try
        {
            await this.paymentService.ProcessPayment(paymentRequest);
        }catch(Exception e)
        {
            logger.LogError("[ProcessPayment] Error catch: {0}", e.Message);
        }
    }

}

