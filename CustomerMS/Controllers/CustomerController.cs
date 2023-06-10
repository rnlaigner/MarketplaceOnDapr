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

    [HttpPost("/")]
    [ProducesResponseType((int)HttpStatusCode.Created)]
    public IActionResult AddCustomer([FromBody] Customer customer)
    {
        this.logger.LogInformation("[AddCustomer] received for id {0}", customer.id);
        this.customerService.AddCustomer(customer);
        this.logger.LogInformation("[AddCustomer] completed for id {0}.", customer.id);
        return StatusCode((int)HttpStatusCode.Created);
    }

    [HttpGet("{customerId}")]
    [ProducesResponseType((int)HttpStatusCode.Found)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public async Task<ActionResult<Customer>> GetCustomers(long customerId)
    {
        this.logger.LogInformation("[GetCustomer] received for seller {0}", customerId);
        Customer customer = await this.customerService.GetCustomer(customerId);
        this.logger.LogInformation("[GetCustomer] completed for seller {0}.", customerId);
        if (customer is not null) return StatusCode((int)HttpStatusCode.Found, customer);
        return NotFound();
    }

}

