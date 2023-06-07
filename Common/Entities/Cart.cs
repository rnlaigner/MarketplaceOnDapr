﻿using System;
using System.Text;

namespace Common.Entities
{

    public class Cart
    {
        // no longer identified within an actor. so it requires an id
        public long customerId { get; set; } = 0;

        public CartStatus status { get; set; } = CartStatus.OPEN;

        public IDictionary<long, CartItem> items { get; set; } = new Dictionary<long, CartItem>();

        public DateTime? createdAt { get; set; }

        public DateTime? updatedAt { get; set; }

        public string instanceId { get; set; } = "";

        // to return
        public List<ProductStatus>? divergencies { get; set; }

        // for dapr
        public Cart() { }

        public Cart(long customerId) {
            this.customerId = customerId;
            this.createdAt = DateTime.Now;
            this.updatedAt = DateTime.Now;
        }

        public override string ToString()
        {
            return new StringBuilder().Append("CustomerId : ").Append(customerId).ToString();
        }

    }
}

