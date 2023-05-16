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

namespace OrderMS.Controllers;

[ApiController]
public class OrderController : ControllerBase
{
    private readonly ILogger<OrderController> logger;
    private readonly OrderService orderService;
    private readonly IOrderRepository orderRepository;

    public OrderController(OrderService orderService, IOrderRepository orderRepository, ILogger<OrderController> logger)
    {
        this.orderService = orderService;
        this.orderRepository = orderRepository;
        this.logger = logger;
    }

    [HttpPost("/checkout")]
    [ProducesResponseType(typeof(Invoice), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.Conflict)]
    [ProducesResponseType((int)HttpStatusCode.MethodNotAllowed)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public async Task<ActionResult<Invoice>> ProcessCheckout(ProcessCheckoutRequest checkout)
    {
        return Ok(new Invoice(new Order(), new List<OrderItem>()));
        // return Ok(this._eventHandler.ProcessCheckout(checkout));
    }

    [HttpGet("/")]
    [ProducesResponseType(typeof(IEnumerable<OrderModel>), (int)HttpStatusCode.OK)]
    public async Task<ActionResult<IEnumerable<OrderModel>>> GetAll()
    {
        // TODO parse http to get filters
        return Ok(this.orderRepository.GetAll());
    }

}

