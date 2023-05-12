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
        this.logger.LogInformation("[ProcessPayment] received: {0}.", paymentRequest.instanceId);
        bool res = await this.paymentService.ProcessPaymentAsync(paymentRequest);
        if (res)
        {
            var paymentRes = new PaymentResult("succeeded", paymentRequest.customer, paymentRequest.order_id, paymentRequest.total_amount, paymentRequest.items, paymentRequest.instanceId);
            await this.daprClient.PublishEventAsync(PUBSUB_NAME, "PaymentResult", paymentRes);

            this.logger.LogInformation("[ProcessPayment] processed: {0}.", paymentRequest.instanceId);
        } else
        {
            // https://stackoverflow.com/questions/73732696/dapr-pubsub-messages-only-being-received-by-one-subscriber
            // https://github.com/dapr/dapr/issues/3176
            // it seems the problem only happens in k8s:
            // https://v1-0.docs.dapr.io/operations/components/component-schema/
            // https://docs.dapr.io/reference/components-reference/supported-pubsub/setup-mqtt3/
            var paymentRes = new PaymentResult("payment_failed", paymentRequest.customer, paymentRequest.order_id, paymentRequest.total_amount, paymentRequest.items, paymentRequest.instanceId);
            await this.daprClient.PublishEventAsync(PUBSUB_NAME, "PaymentResult", paymentRes);
            this.logger.LogInformation("[ProcessPayment] failed: {0}.", paymentRequest.instanceId);
        }
        
    }

}

