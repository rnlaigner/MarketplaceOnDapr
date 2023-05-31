﻿using System.Net;
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

    [HttpPost]
    [ProducesResponseType((int)HttpStatusCode.Created)]
    public IActionResult AddCustomer([FromBody] Customer customer)
    {
        this.logger.LogInformation("[AddCustomer] received for id {0}", customer.id);
        this.customerService.AddCustomer(customer);
        this.logger.LogInformation("[AddCustomer] completed for id {0}.", customer.id);
        return StatusCode((int)HttpStatusCode.Created);
    }

    [HttpGet(Name = "GetCustomer")]
    [ProducesResponseType((int)HttpStatusCode.Found)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public async Task<ActionResult<Customer>> GetCustomerAsync([FromBody] long id)
    {
        this.logger.LogInformation("[GetCustomer] received for seller {0}", id);
        Customer customer = await this.customerService.GetCustomer(id);
        this.logger.LogInformation("[GetCustomer] completed for seller {0}.", id);
        if (customer is not null) return StatusCode((int)HttpStatusCode.Found, customer);
        return NotFound();
    }

    [HttpPost("ProcessPaymentConfirmed")]
    [Topic(PUBSUB_NAME, nameof(PaymentConfirmed))]
    public void ProcessPaymentConfirmed([FromBody] PaymentConfirmed paymentConfirmed)
    {
        this.logger.LogInformation("[ProcessPaymentConfirmed] received for customer {0}", paymentConfirmed.customer.CustomerId);
        this.customerService.ProcessPaymentConfirmed(paymentConfirmed);
        this.logger.LogInformation("[ProcessPaymentConfirmed] completed for customer {0}.", paymentConfirmed.customer.CustomerId);
    }

    [HttpPost("ProcessPaymentFailed")]
    [Topic(PUBSUB_NAME, nameof(PaymentFailed))]
    public void ProcessPaymentFailed([FromBody] PaymentFailed paymentFailed)
    {
        this.logger.LogInformation("[ProcessPaymentConfirmed] received for customer {0}", paymentFailed.customer.CustomerId);
        this.customerService.ProcessPaymentFailed(paymentFailed);
        this.logger.LogInformation("[ProcessPaymentConfirmed] completed for customer {0}.", paymentFailed.customer.CustomerId);
    }

    [HttpPost("ProcessDeliveryNotification")]
    [Topic(PUBSUB_NAME, nameof(DeliveryNotification))]
    public void ProcessDeliveryNotification([FromBody] DeliveryNotification deliveryNotification)
    {
        this.logger.LogInformation("[ProcessDeliveryNotification] received for customer {0}", deliveryNotification.customerId);
        this.customerService.ProcessDeliveryNotification(deliveryNotification);
        this.logger.LogInformation("[ProcessDeliveryNotification] completed for customer {0}.", deliveryNotification.customerId);
    }

}

