using System;

namespace Common.Entities
{
    public record CustomerCheckoutFailed(
        long CustomerId,
        IList<ProductStatus> divergencies
    ); 
}