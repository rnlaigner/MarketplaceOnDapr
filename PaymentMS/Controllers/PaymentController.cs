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
        try
        {
            await this.paymentService.ProcessPayment(invoice);
        }
        catch (Exception)
        {
            await this.paymentService.ProcessPoisonPayment(invoice);
        }
        return Ok();
    }

    [HttpGet]
    [Route("{orderId}")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public ActionResult<IEnumerable<OrderPaymentModel>> GetPaymentByOrderId(int orderId)
    {
        var res = this.paymentRepository.GetByOrderId(orderId);
        return res is not null ? Ok( res ) : NotFound();
    }

    [Route("/cleanup")]
    [HttpPatch]
    [ProducesResponseType((int)HttpStatusCode.Accepted)]
    public ActionResult Cleanup()
    {
        logger.LogWarning("Cleanup requested at {0}", DateTime.UtcNow);
        this.paymentService.Cleanup();
        return Ok();
    }

}

