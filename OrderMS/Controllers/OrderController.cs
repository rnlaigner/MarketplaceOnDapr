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

    [HttpGet("{customerId}")]
    [ProducesResponseType(typeof(IEnumerable<Order>), (int)HttpStatusCode.OK)]
    public ActionResult<IEnumerable<Order>> GetByCustomerId(long customerId)
    {
        // TODO parse http to get filters
        return Ok(this.orderRepository.GetByCustomerId(customerId));
    }

}

