using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using System.Transactions;
using CartMS.Repositories;
using Common.Entities;
using Common.Events;
using Dapr;
using Dapr.Client;
using Microsoft.AspNetCore.Mvc;

namespace CartMS.Controllers;

[ApiController]
public class CartController : ControllerBase
{

    private readonly ILogger<CartController> logger;
    private readonly ICartRepository cartRepository;

    public CartController(ICartRepository cartRepository, ILogger<CartController> logger)
    {
        this.cartRepository = cartRepository;
        this.logger = logger;
    }

    [Route("{customerId}/add")]
    [HttpPatch]
    [ProducesResponseType((int)HttpStatusCode.Accepted)]
    [ProducesResponseType((int)HttpStatusCode.MethodNotAllowed)]
    [ProducesResponseType((int)HttpStatusCode.Conflict)]
    public async Task<ActionResult> AddProduct(string customerId, [FromBody] CartItem item)
    {
        // check if it is already on the way to checkout.... if so, cannot add product
        var cart = await this.cartRepository.GetCart(customerId);
        if (cart != null && cart.status == CartStatus.CHECKOUT_SENT)
            return StatusCode((int)HttpStatusCode.MethodNotAllowed, "Cart " + cart.customerId + " already sent for checkout.");

        this.logger.LogInformation("Customer {0} received request for adding item.", customerId);
        if (item.Quantity <= 0)
        {
            return StatusCode((int)HttpStatusCode.MethodNotAllowed, "Item " + item.ProductId + " shows no positive value.");
        }
        bool res = await this.cartRepository.AddItem(customerId, item);
        if (res)
        {
            this.logger.LogInformation("Customer {0} added item successfully.", customerId);
            return Ok();
        }

        return Conflict();
    }

    [HttpGet("{customerId}")]
    [ProducesResponseType(typeof(Common.Entities.Cart), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public async Task<ActionResult<Common.Entities.Cart>> Get(string customerId)
    {
        var cart = await this.cartRepository.GetCart(customerId);
        if (cart is null)
            return NotFound();
        return Ok(cart);
    }

    /*
     * API for workflow
     * The workflow program will build the Checkout object
     */
    [Route("{customerId}/checkout")]
    [HttpPatch]
    [ProducesResponseType(typeof(Common.Entities.Cart), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.Conflict)]
    [ProducesResponseType((int)HttpStatusCode.MethodNotAllowed)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public async Task<ActionResult<Common.Entities.Cart>> NotifyCheckout(CheckoutNotification checkoutNotification)
    {
        var cart = await this.cartRepository.GetCart(checkoutNotification.customerId);
        if (cart is null)
            return NotFound();

        // workflow calling again
        if (checkoutNotification.instanceId.Equals(cart.instanceId) && cart.status == CartStatus.CHECKOUT_SENT)
        {
            return cart;
        }

        if (cart.status == CartStatus.CHECKOUT_SENT)
        {
            return StatusCode((int)HttpStatusCode.MethodNotAllowed,
                "Cart for customer " + checkoutNotification.customerId + " is on checkout process already!");
        }

        cart.status = CartStatus.CHECKOUT_SENT;
        cart.instanceId = checkoutNotification.instanceId;

        var res = await this.cartRepository.Checkout(cart);
        if (res) return Ok(cart);
        return Conflict();
    }

    [Route("{customerId}/seal")]
    [HttpPatch]
    [ProducesResponseType((int)HttpStatusCode.Accepted)]
    [ProducesResponseType((int)HttpStatusCode.MethodNotAllowed)]
    public async Task<IActionResult> Seal(string customerId)
    {
        var cart = await this.cartRepository.GetCart(customerId);
        if (cart.status != CartStatus.CHECKOUT_SENT)
        {
            return StatusCode((int)HttpStatusCode.MethodNotAllowed);
        }
        await this.cartRepository.Seal(cart);
        return Ok();
    }

    [Route("test/{customerId}")]
    [HttpPatch]
    [ProducesResponseType(typeof(Common.Entities.Cart), (int)HttpStatusCode.OK)]
    public async Task<ActionResult<Common.Entities.Cart>> Test(CheckoutNotification checkoutNotification)
    {
        return Ok(new Cart(checkoutNotification.customerId));
    }

    [Route("test")]
    [HttpGet]
    [ProducesResponseType(typeof(Common.Entities.Cart), (int)HttpStatusCode.OK)]
    public async Task<ActionResult<Common.Entities.Cart>> Test()
    {
        return Ok(new Cart("1"));
    }

}
