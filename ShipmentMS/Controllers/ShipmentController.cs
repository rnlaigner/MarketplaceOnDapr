using Common.Entities;
using System.Net;
using Common.Events;
using Dapr;
using Dapr.Client;
using Microsoft.AspNetCore.Mvc;
using ShipmentMS.Service;
using ShipmentMS.Repositories;
using ShipmentMS.Models;

namespace ShipmentMS.Controllers;

[ApiController]
public class ShipmentController : ControllerBase
{
    private const string PUBSUB_NAME = "pubsub";

    private readonly IShipmentService shipmentService;
    private readonly IShipmentRepository shipmentRepository;
    private readonly ILogger<ShipmentController> logger;

    public ShipmentController(IShipmentService shipmentService, IShipmentRepository shipmentRepository, ILogger<ShipmentController> logger)
    {
        this.shipmentService = shipmentService;
        this.shipmentRepository = shipmentRepository;
        this.logger = logger;
    }

    [HttpGet]
    [Route("{orderId}")]
    [ProducesResponseType(typeof(Shipment), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public ActionResult<Shipment> GetShipment(long orderId)
    {
        this.logger.LogInformation("[GetShipment] received for item id {0}", orderId);
        ShipmentModel? shipment = this.shipmentRepository.GetById(orderId);
        this.logger.LogInformation("[GetShipment] completed for item id {0}.", orderId);
        if (shipment is not null)
            return Ok(new Shipment()
            {
                order_id = shipment.order_id,
                customer_id = shipment.customer_id,
                package_count = shipment.package_count,
                total_freight_value = shipment.total_freight_value,
                request_date = shipment.request_date,
                status = shipment.status,
                first_name = shipment.first_name,
                last_name = shipment.last_name,
                street = shipment.street,
                complement = shipment.complement,
                zip_code = shipment.zip_code,
                city = shipment.zip_code,
                state = shipment.state
            });
        return NotFound();
    }

    [HttpPost("ProcessShipment")]
    [Topic(PUBSUB_NAME, nameof(PaymentConfirmed))]
    public async Task<ActionResult> ProcessShipment([FromBody] PaymentConfirmed paymentRequest)
    {
        this.logger.LogInformation("[ProcessShipment] received for order id {0}", paymentRequest.orderId);
        await this.shipmentService.ProcessShipment(paymentRequest);
        this.logger.LogInformation("[ProcessShipment] succeeded for order id {0}", paymentRequest.orderId);
        return Ok();
    }

    [HttpPatch("UpdateShipment")]
    [Route("/updateShipment/{instanceId}")]
    [ProducesResponseType(typeof(Shipment), (int)HttpStatusCode.Accepted)]
    public async Task<ActionResult> UpdateShipment(string instanceId)
    {
        this.logger.LogInformation("[UpdateShipment] received.");
        await this.shipmentService.UpdateShipment(instanceId);
        this.logger.LogInformation("[UpdateShipment] completed.");
        return Accepted();
    }

}
