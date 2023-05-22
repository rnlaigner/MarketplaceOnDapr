using System.Linq;
using System.Text.Json;
using Common.Entities;
using Common.Events;
using Dapr;
using Dapr.AspNetCore;
using Dapr.Client;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SellerMS.Repositories;
using SellerMS.Services;

namespace CartMS.Controllers;

[ApiController]
public class EventController : ControllerBase
{
    private const string PUBSUB_NAME = "pubsub";

    private readonly ILogger<EventController> logger;
    private readonly ISellerService sellerService;

    public EventController(ISellerService sellerService, 
                           ILogger<EventController> logger)
    {
        this.sellerService = sellerService;
        this.logger = logger;
    }

    [HttpPost("ProcessPaymentConfirmation")]
    [Topic(PUBSUB_NAME, nameof(PaymentConfirmed))]
    public void ProcessPaymentConfirmation([FromBody] PaymentConfirmed paymentRequest)
    {

    }

    [HttpPost("ProcessNewInvoice")]
    [Topic(PUBSUB_NAME, nameof(Common.Events.InvoiceIssued))]
    public async void ProcessNewInvoice(InvoiceIssued paymentRequest)
    {

    }

    // TODO customer ms. also relational. notifications.store behavior of customer over time. customer dashboard

}

