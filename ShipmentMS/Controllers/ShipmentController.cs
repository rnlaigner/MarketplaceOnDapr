using Common.Events;
using Dapr;
using Dapr.Client;
using Microsoft.AspNetCore.Mvc;
using ShipmentMS.Service;

namespace ShipmentMS.Controllers;

[ApiController]
public class ShipmentController : ControllerBase
{
    private const string PUBSUB_NAME = "pubsub";

    private readonly DaprClient daprClient;
    private readonly IShipmentService shipmentService;

    private readonly ILogger<ShipmentController> logger;

    public ShipmentController(IShipmentService shipmentService, DaprClient daprClient, ILogger<ShipmentController> logger)
    {
        this.shipmentService = shipmentService;
        this.daprClient = daprClient;
        this.logger = logger;
    }

    [HttpPost("ProcessShipment")]
    [Topic(PUBSUB_NAME, nameof(PaymentResult))]
    public void ProcessShipment(PaymentResult paymentRequest)
    {
        this.shipmentService.ProcessShipment(paymentRequest);
    }
}
