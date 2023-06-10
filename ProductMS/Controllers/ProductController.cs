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
            this.logger.LogInformation("[GetBySeller] returning seller {0}", sellerId);
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
            this.logger.LogInformation("[GetById] returning product {0}", productId);
            return Ok(new Product()
            {
                seller_id = product.seller_id,
                product_id= product.product_id,
                name = product.name,
                sku = product.sku,
                category = product.category,
                description = product.description,
                price = product.price,
                freight_value = product.freight_value,
                status = product.status,
                active = product.active
            });
        }
        return NotFound();
    }

    [HttpDelete]
    [Route("{productId}")]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    [ProducesResponseType((int)HttpStatusCode.Accepted)]
    public async Task<ActionResult> DeleteById(long productId)
    {
        var product = this.productRepository.GetProduct(productId);
        if (product != null) { 
            await this.productService.Delete(product);
            return Accepted();
        }
        return NotFound();
    }

    [HttpPost]
    [Route("/")]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    [ProducesResponseType((int)HttpStatusCode.Created)]
    public async Task<ActionResult> AddProduct([FromBody] Product product)
    {
        bool res = await this.productService.Upsert(product);
        if (res)
        {
            return StatusCode((int)HttpStatusCode.Created);
        }
        return NotFound();
    }

    [HttpPut]
    [Route("/")]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    [ProducesResponseType((int)HttpStatusCode.Accepted)]
    public async Task<ActionResult> UpdateProduct([FromBody] Product product)
    {
        bool res = await this.productService.Upsert(product);
        if (res)
        {
            return Accepted();
        }
        return NotFound();
    }

}