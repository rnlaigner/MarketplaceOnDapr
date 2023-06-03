using System.Net;
using CartMS.Repositories;
using CartMS.Services;
using Common.Entities;
using Common.Events;
using Microsoft.AspNetCore.Mvc;

namespace CartMS.Controllers;

[ApiController]
public class CartController : ControllerBase
{

    private readonly ILogger<CartController> logger;
    private readonly ICartService cartService;
    private readonly ICartRepository cartRepository;

    public CartController(ICartService cartService, ICartRepository cartRepository, ILogger<CartController> logger)
    {
        this.cartService = cartService;
        this.cartRepository = cartRepository;
        this.logger = logger;
    }

    //[HttpGet("/healthcheck")]
    //[ProducesResponseType((int)HttpStatusCode.OK)]
    //[ProducesResponseType((int)HttpStatusCode.InternalServerError)]
    //public ActionResult Healthcheck()
    //{
    //    return Ok();
    //}

    [Route("{customerId}")]
    [HttpPatch]
    [ProducesResponseType((int)HttpStatusCode.Accepted)]
    [ProducesResponseType((int)HttpStatusCode.MethodNotAllowed)]
    [ProducesResponseType((int)HttpStatusCode.Conflict)]
    public async Task<ActionResult> AddProduct(long customerId, [FromBody] CartItem item)
    {
        // check if it is already on the way to checkout.... if so, cannot add product
        var cart = await this.cartRepository.GetCart(customerId);
        if (cart != null && cart.status == CartStatus.CHECKOUT_SENT)
            return StatusCode((int)HttpStatusCode.MethodNotAllowed, "Cart " + cart.customerId + " already sent for checkout.");

        this.logger.LogInformation("Customer {0} received request for adding item.", customerId);
        if (item.Quantity <= 0)
        {
            return StatusCode((int)HttpStatusCode.MethodNotAllowed, "Item " + item.ProductId + " shows no positive quantity.");
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
    [ProducesResponseType(typeof(Cart), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public async Task<ActionResult<Cart>> Get(long customerId)
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
    [Route("workflowCheckout")]
    [HttpPost]
    [ProducesResponseType(typeof(Cart), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.Conflict)]
    [ProducesResponseType((int)HttpStatusCode.MethodNotAllowed)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public async Task<ActionResult<Cart>> NotifyCheckout(CheckoutNotification checkoutNotification)
    {
        // querying the kv store
        var cart = await this.cartRepository.GetCart(checkoutNotification.customerId);
        // in the database, this would be like: select count(*) from cart where cart.instanceId = ?
        if (cart is null)
            return NotFound();

        if(cart.items.Count() == 0)
        {
            return StatusCode((int)HttpStatusCode.MethodNotAllowed,
                "Cart for customer " + checkoutNotification.customerId + " shows no items to checkout!");
        }

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

        var divergencies = await cartService.CheckCartForDivergencies(cart);
        if (divergencies.Count() > 0)
        {
            cart.divergencies = divergencies;
            await this.cartRepository.Save(cart);
            return StatusCode((int)HttpStatusCode.MethodNotAllowed, cart);
        }
        else
        {
            cart.instanceId = checkoutNotification.instanceId;
            cart.status = CartStatus.CHECKOUT_SENT;
        }
        var res = await this.cartRepository.SafeSave(cart);
        if (res)
        {
            return Ok(cart);
        }
        return Conflict();
    }

    [Route("{customerId}/seal")]
    [HttpPatch]
    [ProducesResponseType((int)HttpStatusCode.Accepted)]
    [ProducesResponseType((int)HttpStatusCode.MethodNotAllowed)]
    public async Task<IActionResult> Seal(long customerId)
    {
        var cart = await this.cartRepository.GetCart(customerId);
        if (cart.status != CartStatus.CHECKOUT_SENT)
        {
            return StatusCode((int)HttpStatusCode.MethodNotAllowed, "Cart is not in CHECKOUT_SENT status");
        }
        /**
         * Seal is a terminal state, so no need to check for concurrent operation
         */
        await this.cartService.Seal(cart);
        return Ok();
    }

    [Route("test/{customerId}")]
    [HttpPatch]
    [ProducesResponseType(typeof(Cart), (int)HttpStatusCode.OK)]
    public ActionResult<Cart> Test(CheckoutNotification checkoutNotification)
    {
        this.logger.LogInformation("[Test] received request for customer {0}.", checkoutNotification.customerId);
        return Ok(new Cart(checkoutNotification.customerId));
    }

    [Route("checkout")]
    [HttpPatch]
    [ProducesResponseType((int)HttpStatusCode.Accepted)]
    [ProducesResponseType((int)HttpStatusCode.MethodNotAllowed)]
    public async Task<IActionResult> NotifyCheckout([FromBody] CustomerCheckout customerCheckout)
    {
        this.logger.LogInformation("[NotifyCheckout] received request.");
        Cart cart = await this.cartRepository.GetCart(customerCheckout.CustomerId);
        if (cart.status == CartStatus.CHECKOUT_SENT)
        {
            this.logger.LogWarning("Customer {0} cart has already been submitted to checkout", customerCheckout.CustomerId);
            return StatusCode((int)HttpStatusCode.MethodNotAllowed, "Customer "+ customerCheckout.CustomerId + " cart has already been submitted to checkout");
        }
        await this.cartService.NotifyCheckout(customerCheckout, cart);
        return Ok();
    }

}
