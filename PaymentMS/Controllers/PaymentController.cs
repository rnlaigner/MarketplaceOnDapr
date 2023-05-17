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
    [Topic(PUBSUB_NAME, nameof(Common.Events.ProcessPayment), "poisonMessages", false)]
    public async void ProcessPayment(ProcessPayment paymentRequest)
    {
        this.logger.LogInformation("[ProcessPayment] received: {0}.", paymentRequest.instanceId);
        await this.paymentService.ProcessPayment(paymentRequest);
    }

}

