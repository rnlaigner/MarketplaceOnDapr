using System.Net;
using Common.Entities;
using Common.Events;
using CustomerMS.Services;
using Dapr;
using Microsoft.AspNetCore.Mvc;

namespace CustomerMS.Controllers;

[ApiController]
public class CustomerController : ControllerBase
{

    private const string PUBSUB_NAME = "pubsub";

    private readonly ICustomerService customerService;
    private readonly ILogger<CustomerController> logger;

    public CustomerController(ICustomerService customerService, ILogger<CustomerController> logger)
    {
        this.customerService = customerService;
        this.logger = logger;
    }

    /*
    [HttpGet(Name = "GetCustomers")]
    public IEnumerable<Customer> Get()
    {
        return Ok();
    }
    */

    [HttpPost]
    [ProducesResponseType((int)HttpStatusCode.Created)]
    public IActionResult AddCustomer([FromBody] Customer customer, [FromHeader(Name = "instanceId")] string instanceId)
    {
        this.logger.LogInformation("[AddCustomer] received for instanceId {0}", instanceId);


        this.customerService.AddCustomer(customer);

        this.logger.LogInformation("[AddCustomer] completed for instanceId {0}.", instanceId);
        return StatusCode((int)HttpStatusCode.Created);
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

}

