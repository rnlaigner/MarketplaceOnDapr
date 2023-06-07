using System.Net;
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
    [Route("sellers/{sellerId}")]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType(typeof(Product), (int)HttpStatusCode.OK)]
    public ActionResult<Product> GetBySeller(long sellerId)
    {
        this.logger.LogInformation("[GetBySeller] received for seller {0}", sellerId);
        if (sellerId <= 0)
        {
            return BadRequest();
        }
        var product = this.productRepository.GetBySeller(sellerId);

        if (product != null)
        {
            this.logger.LogInformation("[GetBySeller] returning for product {0}", sellerId);
            return Ok(product);
        }
        return NotFound();
    }

    [HttpGet]
    [Route("{productId}")]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType(typeof(Product), (int)HttpStatusCode.OK)]
    public ActionResult<Product> GetById(long productId)
    {
        this.logger.LogInformation("[GetById] received for product {0}", productId);
        if (productId <= 0)
        {
            return BadRequest();
        }
        var product = this.productRepository.GetProduct(productId);

        if (product != null)
        {
            this.logger.LogInformation("[GetById] returning for product {0}", productId);
            return Ok(product);
        }
        return NotFound();
    }

    [HttpDelete]
    [Route("{productId}")]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    [ProducesResponseType((int)HttpStatusCode.Accepted)]
    public ActionResult DeleteById(long productId)
    {
        var product = this.productRepository.GetProduct(productId);
        if (product != null) { 
            this.productService.Delete(product);
            return Accepted();
        }
        return NotFound();
    }

    [HttpPost("/")]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    [ProducesResponseType((int)HttpStatusCode.Created)]
    public async Task<IActionResult> AddProduct([FromBody] Product product)
    {
        bool res = await this.productService.Upsert(product);
        if (res)
        {
            return StatusCode((int)HttpStatusCode.Created);
        }
        return NotFound();
    }

    [HttpPut("/")]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    [ProducesResponseType((int)HttpStatusCode.Accepted)]
    public async Task<IActionResult> UpdateProduct([FromBody] Product product)
    {
        bool res = await this.productService.Upsert(product);
        if (res)
        {
            return Accepted();
        }
        return NotFound();
    }

}