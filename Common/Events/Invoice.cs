using System;
using Common.Entities;

namespace Common.Events
{
    public record Invoice
    {
        public readonly Order order;
        public readonly IList<OrderItem> items;

        public Invoice(Order order, IList<OrderItem> items)
        {
            this.order = order;
            this.items = items;
        }
    }
}

