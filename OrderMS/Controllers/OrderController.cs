﻿using System.Net;
using Common.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OrderMS.Common.Repositories;
using System.Collections.Generic;
using OrderMS.Services;
using System;

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
    public ActionResult<IEnumerable<Order>> GetByCustomerId(int customerId)
    {
        return Ok(this.orderRepository.GetByCustomerId(customerId));
    }

    [Route("/cleanup")]
    [HttpPatch]
    [ProducesResponseType((int)HttpStatusCode.Accepted)]
    public ActionResult Cleanup()
    {
        logger.LogWarning("Cleanup requested at {0}", DateTime.UtcNow);
        this.orderService.Cleanup();
        return Ok();
    }

}

