using System;
namespace Common.Entities
{
    /**
     * Ideally it would be nice to separate the cart item submitted by the customer worker
     * and the actual data object stored by the cart. In other words,
     * some data is not necessary for the customer to know about, such as
     * oldunitprice, sellerid, sku, these are assembled on checkout based on the stored/replicated product information
     * The customer must submit an object with:
     * vouchers, productid, unitprice, 
     */
     public record CartItem
     (
         long ProductId,
         string Name, // will be used downstream by customer
         string Sku, // used to match the replicated products in cart
         string Category, // used by seller for dashboard
         long SellerId,
         decimal UnitPrice,
         decimal OldUnitPrice,
         decimal FreightValue,
         int Quantity,
        // Vouchers to be applied
        // coupons for different sellers, usually attached to products
        // but we don't formally track these in the benchmark
        decimal[] Vouchers
     );
}

