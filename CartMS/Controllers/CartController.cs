using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using CartMS.Models;
using CartMS.Repositories;
using CartMS.Services;
using Common.Entities;
using Common.Events;
using Common.Integration;
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
    public ActionResult AddItem(long customerId, [FromBody] CartItem item)
    {
        this.logger.LogInformation("Customer {0} received request for adding item.", customerId);
        if (item.Quantity <= 0)
        {
            return StatusCode((int)HttpStatusCode.MethodNotAllowed, "Item " + item.ProductId + " shows no positive quantity.");
        }

        // check if it is already on the way to checkout.... if so, cannot add product
        var cart = this.cartRepository.GetCart(customerId);
        if (cart != null && cart.status == CartStatus.CHECKOUT_SENT)
        {
            this.logger.LogWarning("Cart for customer {0} already sent for checkout.", customerId);
            return StatusCode((int)HttpStatusCode.MethodNotAllowed, "Cart for customer " + cart.customer_id + " already sent for checkout.");
        }

        if (cart is null) {
            cart = cartRepository.Insert(new()
            {
                customer_id = customerId,
            });
        }

        string? vouchersInput = null;
        if(item.Vouchers is not null && item.Vouchers.Count() > 0)
        {
            vouchersInput = String.Join(",", item.Vouchers.Select(v => v.ToString(CultureInfo.InvariantCulture)));
        }

        CartItemModel cartItemModel = new()
        {
            customer_id = customerId,
            seller_id = item.SellerId,
            product_id = item.ProductId,
            product_name = item.ProductName,
            unit_price = item.UnitPrice,
            freight_value = item.FreightValue,
            quantity = item.Quantity,
            vouchers = vouchersInput
        };

        this.cartRepository.AddItem(cartItemModel);
        this.logger.LogInformation("Customer {0} added item successfully.", customerId);
        return Accepted();
    }

    private static readonly decimal[] emptyArray = Array.Empty<decimal>();

    [HttpGet("{customerId}")]
    [ProducesResponseType(typeof(Cart), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public ActionResult<Cart> Get(long customerId)
    {
        var cart = this.cartRepository.GetCart(customerId);
        if (cart is null)
            return NotFound();

        var items = cartRepository.GetItems(customerId);

        // https://stackoverflow.com/questions/9524682/

        var cartItems = items.Select(i => new CartItem()
        {
            SellerId = i.seller_id,
            ProductId = i.product_id,
            ProductName = i.product_name,
            UnitPrice = i.unit_price,
            FreightValue = i.freight_value,
            Quantity = i.quantity,
            Vouchers = i.vouchers is null ? emptyArray : Array.ConvertAll(i.vouchers.Split(','), decimal.Parse)
        }).ToList();

        return Ok(new Cart()
        {
            customerId = cart.customer_id,
            items = cartItems,
            status = cart.status,
        });
    }

    [HttpDelete("{customerId}")]
    [ProducesResponseType((int)HttpStatusCode.Accepted)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public ActionResult Delete(long customerId)
    {
        this.logger.LogInformation("Customer {0} requested to delete cart.", customerId);
        var cart = this.cartRepository.Delete(customerId);
        if (cart is null)
            return NotFound();
        return Accepted();
    }

    [Route("{customerId}/seal")]
    [HttpPatch]
    [ProducesResponseType((int)HttpStatusCode.Accepted)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public ActionResult Seal(long customerId)
    {
        var cart = this.cartRepository.GetCart(customerId);
        if(cart is null)
        {
            return NotFound();
        }

        /**
         * Seal is a terminal state, so no need to check for concurrent operation
         */
        this.cartService.Seal(cart);
        return Accepted();
    }

    [Route("{customerId}/checkout")]
    [HttpPost]
    [ProducesResponseType((int)HttpStatusCode.Accepted)]
    [ProducesResponseType(typeof(Cart),(int)HttpStatusCode.MethodNotAllowed)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public async Task<ActionResult> NotifyCheckout(long customerId, [FromBody] CustomerCheckout customerCheckout)
    {
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
            return NotFound("Customer "+ customerCheckout.CustomerId + " cart cannot be found");
        }

        if (cart.status == CartStatus.CHECKOUT_SENT)
        {
            this.logger.LogWarning("Customer {0} cart has already been submitted to checkout", customerCheckout.CustomerId);
            return StatusCode((int)HttpStatusCode.MethodNotAllowed, "Customer "+ customerCheckout.CustomerId + " cart has already been submitted for checkout");
        }

        var items = this.cartRepository.GetItems(customerCheckout.CustomerId);
        if(items is null || items.Count == 0)
        {
            return StatusCode((int)HttpStatusCode.MethodNotAllowed, "Customer " + customerCheckout.CustomerId + " cart has no items to be submitted for checkout");
        }

        List<ProductStatus> divergencies = this.cartService.CheckCartForDivergencies(cart);
        if (divergencies.Count() > 0)
        {
            return StatusCode((int)HttpStatusCode.MethodNotAllowed, new Cart()
            {
                customerId = cart.customer_id,
                // items = cartItems,
                status = cart.status,
                divergencies = divergencies
            });
        }

        await this.cartService.NotifyCheckout(customerCheckout, cart);
        return Ok();
    }

}