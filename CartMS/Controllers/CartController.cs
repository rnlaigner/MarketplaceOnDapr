using System.Net;
using CartMS.Models;
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

    [Route("{customerId}/add")]
    [HttpPatch]
    [ProducesResponseType((int)HttpStatusCode.Accepted)]
    [ProducesResponseType((int)HttpStatusCode.MethodNotAllowed)]
    [ProducesResponseType((int)HttpStatusCode.Conflict)]
    public ActionResult AddProduct(long customerId, [FromBody] CartItem item)
    {
        // check if it is already on the way to checkout.... if so, cannot add product
        var cart = this.cartRepository.GetCart(customerId);
        if (cart != null && cart.status == CartStatus.CHECKOUT_SENT)
        {
            this.logger.LogWarning("Cart for customer {0} already sent for checkout.", customerId);
            return StatusCode((int)HttpStatusCode.MethodNotAllowed, "Cart for customer " + cart.customer_id + " already sent for checkout.");
        }

        this.logger.LogInformation("Customer {0} received request for adding item.", customerId);
        if (item.Quantity <= 0)
        {
            return StatusCode((int)HttpStatusCode.MethodNotAllowed, "Item " + item.ProductId + " shows no positive quantity.");
        }

        if (cart is null) {
            cart = cartRepository.Insert(new()
            {
                customer_id = customerId
            });
        }

        CartItemModel cartItemModel = new();
        var res = this.cartRepository.AddItem(cartItemModel);
        if (res is not null)
        {
            this.logger.LogInformation("Customer {0} added item successfully.", customerId);
            return Ok();
        }

        return Conflict();
    }

    [HttpGet("{customerId}")]
    [ProducesResponseType(typeof(Cart), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public ActionResult<Cart> Get(long customerId)
    {
        var cart = this.cartRepository.GetCart(customerId);
        if (cart is null)
            return NotFound();

        var items = cartRepository.GetItems(customerId);

        var cartItems = items.Select(i => new CartItem()
        {
            SellerId = i.seller_id,
            ProductId = i.product_id,
            ProductName = i.product_name,
            UnitPrice = i.unit_price,
            FreightValue = i.freight_value,
            Quantity = i.quantity,
            Vouchers = i.vouchers
        }).ToList();

        return Ok(new Cart()
        {
            items = cartItems
        });
    }

    [HttpDelete("{customerId}")]
    [ProducesResponseType(typeof(Cart), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public ActionResult<Cart> Delete(long customerId)
    {
        this.logger.LogInformation("Customer {0} requested to delete cart.", customerId);
        var cart = this.cartRepository.Delete(customerId);
        if (cart is null)
            return NotFound();
        // FIXME parse
        return Ok(cart);
    }

    [Route("{customerId}/seal")]
    [HttpPatch]
    [ProducesResponseType((int)HttpStatusCode.Accepted)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    [ProducesResponseType((int)HttpStatusCode.MethodNotAllowed)]
    public ActionResult Seal(long customerId)
    {
        var cart = this.cartRepository.GetCart(customerId);
        if(cart is null)
        {
            return NotFound();
        }
        if (cart.status != CartStatus.CHECKOUT_SENT)
        {
            return StatusCode((int)HttpStatusCode.MethodNotAllowed, "Cart is not in CHECKOUT_SENT status");
        }
        /**
         * Seal is a terminal state, so no need to check for concurrent operation
         */
        this.cartService.Seal(cart);
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

    [Route("{customerId}/checkout")]
    [HttpPost]
    [ProducesResponseType((int)HttpStatusCode.Accepted)]
    [ProducesResponseType((int)HttpStatusCode.MethodNotAllowed)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public async Task<ActionResult> NotifyCheckout(long customerId, [FromBody] CustomerCheckout customerCheckout)
    {
        // TODO log a transaction event
        this.logger.LogInformation("[NotifyCheckout] received request.");

        if (customerId != customerCheckout.CustomerId)
        {
            logger.LogError("Customer checkout payload ({0}) does not match customer ID ({1}) in URL", customerId, customerCheckout.CustomerId);
            return StatusCode((int)HttpStatusCode.MethodNotAllowed, "Customer checkout payload does not match customer ID in URL");
        }
        
        CartModel? cart = this.cartRepository.GetCart(customerCheckout.CustomerId);

        if(cart is null)
        {
            this.logger.LogWarning("Customer {0} cart cannot be found", customerCheckout.CustomerId);
            return NotFound();
        }

        if (cart.status == CartStatus.CHECKOUT_SENT)
        {
            this.logger.LogWarning("Customer {0} cart has already been submitted to checkout", customerCheckout.CustomerId);
            return StatusCode((int)HttpStatusCode.MethodNotAllowed, "Customer "+ customerCheckout.CustomerId + " cart has already been submitted to checkout");
        }

        await this.cartService.NotifyCheckout(customerCheckout, cart);
        return Ok();
    }

}
