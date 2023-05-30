using System.Diagnostics;
using System.Net;
using Common.Entities;
using Common.Events;
using Microsoft.AspNetCore.Mvc;
using OrderMS.Handlers;
using OrderMS.Infra;
using OrderMS.Common.Models;
using OrderMS.Repositories;
using Microsoft.Extensions.Logging;
using OrderMS.Common.Repositories;
using System.Threading.Tasks;
using System.Collections.Generic;
using OrderMS.Services;

namespace OrderMS.Controllers;

[ApiController]
public class OrderController : ControllerBase
{
    private readonly ILogger<OrderController> logger;
    private readonly IOrderService orderService;
    private readonly IOrderRepository orderRepository;

    public OrderController(IOrderService orderService, IOrderRepository orderRepository, ILogger<OrderController> logger)
    {
        this.orderService = orderService;
        this.orderRepository = orderRepository;
        this.logger = logger;
    }

    [HttpPost("/checkout")]
    [ProducesResponseType(typeof(InvoiceIssued), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.Conflict)]
    [ProducesResponseType((int)HttpStatusCode.MethodNotAllowed)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public async Task<ActionResult<InvoiceIssued>> ProcessCheckout(StockConfirmed checkout)
    {
        return Ok();
        // return Ok(this._eventHandler.ProcessCheckout(checkout));
    }

    [HttpGet("/")]
    [ProducesResponseType(typeof(IEnumerable<Order>), (int)HttpStatusCode.OK)]
    public async Task<ActionResult<IEnumerable<Order>>> GetAll()
    {
        // TODO parse http to get filters
        return Ok(this.orderRepository.GetAll());
    }

}

