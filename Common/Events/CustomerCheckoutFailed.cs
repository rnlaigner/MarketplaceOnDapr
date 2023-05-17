using System;

namespace Common.Entities
{

    public record CustomerCheckoutFailed(
        string CustomerId,
        IList<ProductStatus> divergencies
    );
    
}

