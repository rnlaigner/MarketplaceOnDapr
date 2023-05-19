using System;
namespace Common.Entities
{
    /**
     * Entity not present in olist original data set
     * Thus, the basket item entity is derived from
     * the needs to process the order.
     * This could include the freight value...
     */
     public record CartItem
     (
         long ProductId,
         string Name, // will be used downstream by customer
         string Sku, // used to match the replicated products  
         long SellerId,
         decimal UnitPrice,
         decimal OldUnitPrice,
         decimal FreightValue,
         int Quantity
     );
}

