﻿using System.Net;
using Common.Entities;
using Common.Events;
using Dapr;
using Microsoft.AspNetCore.Mvc;
using ProductMS.Repositories;
using ProductMS.Services;

namespace ProductMS.Controllers;

[ApiController]
public class ProductController : ControllerBase
{
    private const string PUBSUB_NAME = "pubsub";

    private readonly ILogger<ProductController> logger;
    private readonly IProductRepository productRepository;
    private readonly IProductService productService;

    public ProductController(IProductService productService, IProductRepository productRepository, ILogger<ProductController> logger)
    {
        this.productService = productService;
        this.logger = logger;
        this.productRepository = productRepository;
    }


    [HttpGet]
    [Route("/")]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType(typeof(Product), (int)HttpStatusCode.OK)]
    public ActionResult<List<Product>> GetAll()
    {
        return Ok();
    }

    /**
     * https://stackoverflow.com/questions/36280947/how-to-pass-multiple-parameters-to-a-get-method-in-asp-net-core
     */
    [HttpGet]
    [Route("{sellerId}/{productId}")]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType(typeof(Product), (int)HttpStatusCode.OK)]
    public ActionResult<Product> GetById(long sellerId, long productId)
    {
        if (sellerId <= 0 || productId <= 0)
        {
            return BadRequest();
        }
        var product = this.productRepository.GetProduct(sellerId, productId);

        if (product != null)
        {
            return Ok(product);
        }
        return NotFound();
    }

    [HttpPost("/")]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    [ProducesResponseType((int)HttpStatusCode.Created)]
    public async Task<IActionResult> AddProduct([FromBody] Product product, [FromHeader(Name = "instanceId")] string instanceId)
    {
        this.logger.LogInformation("[AddProduct] received for instanceId {0}", instanceId);
        bool res = await this.productService.Upsert(product);
        this.logger.LogInformation("[AddProduct] completed for instanceId {0}. Returning {res}", instanceId, res);
        if (res)
        {
            return StatusCode((int)HttpStatusCode.Created);
        }
        return NotFound();
    }

    [HttpPut("/")]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    [ProducesResponseType((int)HttpStatusCode.Accepted)]
    public async Task<IActionResult> UpdateProduct([FromBody] Product product, [FromHeader(Name = "instanceId")] string instanceId)
    {
        this.logger.LogInformation("[UpdateProduct] received for instanceId {0}", instanceId);
        bool res = await this.productService.Upsert(product);
        this.logger.LogInformation("[UpdateProduct] completed for instanceId {0}. Returning {res}", instanceId, res);
        if (res)
        {
            return Accepted();
        }
        return NotFound();
    }

    [HttpPut("/")]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    [ProducesResponseType((int)HttpStatusCode.Accepted)]
    public async Task<IActionResult> DeleteProduct([FromBody] Product product, [FromHeader(Name = "instanceId")] string instanceId)
    {
        this.logger.LogInformation("[UpdateProduct] received for instanceId {0}", instanceId);
        bool res = await this.productService.Delete(product);
        this.logger.LogInformation("[UpdateProduct] completed for instanceId {0}. Returning {res}", instanceId, res);
        if (res)
        {
            return Accepted();
        }
        return NotFound();
    }

}