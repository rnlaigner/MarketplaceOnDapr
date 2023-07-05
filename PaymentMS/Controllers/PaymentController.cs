using System.Net;
using Common.Events;
using Dapr;
using Microsoft.AspNetCore.Mvc;
using PaymentMS.Models;
using PaymentMS.Repositories;
using PaymentMS.Services;

namespace PaymentMS.Controllers;

[ApiController]
public class PaymentController : ControllerBase
{

    private const string PUBSUB_NAME = "pubsub";

    private readonly IPaymentService paymentService;
    private readonly IPaymentRepository paymentRepository;
    private readonly ILogger<PaymentController> logger;
    
    public PaymentController(IPaymentService paymentService, IPaymentRepository paymentRepository, ILogger<PaymentController> logger)
    {
        this.paymentService = paymentService;
        this.paymentRepository = paymentRepository;
        this.logger = logger;
    }

    [HttpPost("ProcessPayment")]
    [Topic(PUBSUB_NAME, nameof(InvoiceIssued))]
    public async Task<ActionResult> ProcessPayment([FromBody] InvoiceIssued invoice)
    {
        this.logger.LogInformation("[InvoiceIssued] received for order ID {0}.", invoice.orderId);
        await this.paymentService.ProcessPayment(invoice);
        return Ok();
    }

    [HttpGet]
    [Route("{orderId}")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public ActionResult<IEnumerable<OrderPaymentModel>> GetPaymentByOrderId(long orderId)
    {
        this.logger.LogInformation("[GetPaymentByOrderId] received for order ID {0}.", orderId);
        var res = this.paymentRepository.GetByOrderId(orderId);
        return res is not null ? Ok( res ) : NotFound();
    }

}

