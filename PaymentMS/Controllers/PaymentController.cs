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

    private readonly DaprClient daprClient;
    private readonly IPaymentService paymentService;

    private readonly ILogger<PaymentController> logger;

    public PaymentController(IPaymentService paymentService, DaprClient daprClient, ILogger<PaymentController> logger)
    {
        this.paymentService = paymentService;
        this.daprClient = daprClient;
        this.logger = logger;
    }

    [HttpPost("ProcessPayment")]
    [Topic(PUBSUB_NAME, nameof(PaymentRequest))]
    public async void ProcessPayment(PaymentRequest paymentRequest)
    {
        this.logger.LogInformation("[ProcessPayment] received {0}.", paymentRequest.instanceId);
        bool res = await this.paymentService.ProcessPaymentAsync(paymentRequest);
        // TODO create shipment request
        // await this.daprClient.PublishEventAsync(PUBSUB_NAME, "CheckoutResult", invoice);
        this.logger.LogInformation("[ProcessPayment] processed {0}.", paymentRequest.instanceId);
    }

}

