using System;
using Common.Entities;

namespace Common.Events
{
    public record IncreaseStock
    (
        long seller_id,

        long product_id,

        int quantity

        
    );
}

