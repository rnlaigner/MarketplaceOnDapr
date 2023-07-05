using System.Net;
using Common.Entities;
using Common.Requests;
using Microsoft.AspNetCore.Mvc;
using ProductMS.Repositories;
using ProductMS.Services;

namespace ProductMS.Controllers;

[ApiController]
public class ProductController : ControllerBase
{
    private readonly ILogger<ProductController> logger;
    private readonly IProductRepository productRepository;
    private readonly IProductService productService;

    public ProductController(IProductService productService, IProductRepository productRepository, ILogger<ProductController> logger)
    {
        this.productService = productService;
        this.logger = logger;
        this.productRepository = productRepository;
    }

    [HttpGet("{sellerId}")]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType(typeof(List<Product>), (int)HttpStatusCode.OK)]
    public ActionResult<List<Product>> GetBySellerId(long sellerId)
    {
        this.logger.LogInformation("[GetBySeller] received for seller {0}", sellerId);
        if (sellerId <= 0)
        {
            return BadRequest();
        }
        var products = this.productRepository.GetBySeller(sellerId);

        if (products is not null && products.Count() > 0)
        {
            List<Product> productsResp = new List<Product>(products.Count());
            foreach(var product in products)
            {
                productsResp.Add(
                new Product()
                {
                    seller_id = product.seller_id,
                    product_id = product.product_id,
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
            this.logger.LogInformation("[GetBySeller] returning seller {0} products...", sellerId);
            return Ok(productsResp);
        }
        return NotFound();
    }

    [HttpGet("{sellerId:long}/{productId:long}")]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType(typeof(Product), (int)HttpStatusCode.OK)]
    public ActionResult<Product> GetBySellerIdAndProductId(long sellerId, long productId)
    {
        this.logger.LogInformation("[GetById] received for product {0}", productId);
        if (productId <= 0)
        {
            return BadRequest();
        }
        var product = this.productRepository.GetProduct(sellerId, productId);

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
    [Route("/")]
    [ProducesResponseType((int)HttpStatusCode.Accepted)]
    public async Task<ActionResult> DeleteProduct([FromBody] DeleteProduct deleteProduct)
    {
        await this.productService.ProcessDelete(deleteProduct);
        return Accepted();
    }

    [HttpPost]
    [Route("/")]
    [ProducesResponseType((int)HttpStatusCode.Created)]
    public async Task<ActionResult> AddProduct([FromBody] Product product)
    {
        await this.productService.ProcessNewProduct(product);
        return StatusCode((int)HttpStatusCode.Created);
    }

    [HttpPatch]
    [Route("/")]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    [ProducesResponseType((int)HttpStatusCode.Accepted)]
    public async Task<ActionResult> UpdateProduct([FromBody] UpdatePrice update)
    {
        await this.productService.ProcessUpdate(update);
        return Accepted();
    }

    [Route("/cleanup")]
    [HttpPatch]
    [ProducesResponseType((int)HttpStatusCode.Accepted)]
    public ActionResult Cleanup()
    {
        this.productService.Cleanup();
        return Ok();
    }

}