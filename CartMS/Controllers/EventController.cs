﻿using System.Linq;
using System.Text.Json;
using CartMS.Infra;
using CartMS.Models;
using CartMS.Repositories;
using CartMS.Services;
using Common.Entities;
using Common.Events;
using Dapr;
using Dapr.AspNetCore;
using Dapr.Client;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace CartMS.Controllers;

[ApiController]
public class EventController : ControllerBase
{
    private const string PUBSUB_NAME = "pubsub";

    private readonly DaprClient daprClient;
    private readonly ICartRepository cartRepository;
    private readonly IProductRepository productRepository;

    private readonly ILogger<EventController> logger;
    private readonly ICartService cartService;

    public EventController(DaprClient daprClient,
                            ICartService cartService, 
                            ICartRepository cartRepository,
                            IProductRepository productRepository,
                            ILogger<EventController> logger)
    {
        this.daprClient = daprClient;
        this.cartRepository = cartRepository;
        this.productRepository = productRepository;
        this.logger = logger;
        this.cartService = cartService;
    }

    [HttpPost("ProductStreaming")]
    [Topic(PUBSUB_NAME, nameof(Product))]
    public ActionResult ProcessProductStream([FromBody] Product product)
    {
        ProductModel? product_ = productRepository.GetProduct(product.seller_id, product.product_id);

        if(product_ is null)
        {
            var now = DateTime.Now;
            product_ = new()
            {
                seller_id = product.seller_id,
                product_id = product.product_id,
                name = product.name,
                sku = product.sku,
                category = product.category,
                description = product.description,
                price = product.price,
                created_at = now,
                updated_at = now,
                status = product.status,
                active = product.active
            };
            this.productRepository.Insert(product_);
        }

        if (product.active)
        {
            this.productRepository.Update(product_);
        } else
        {
            this.productRepository.Delete(product_);
        }


        this.logger.LogInformation("[ProcessProductStream] completed for product ID {0}", product.product_id);
        return Ok();
    }

}