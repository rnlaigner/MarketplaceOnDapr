using System;
namespace Common.Entities
{

    public class Cart
    {
        // no longer identified within an actor. so it requires an id
        public string customerId { get; set; } = "";

        public CartStatus status { get; set; } = CartStatus.OPEN;

        public IDictionary<long, CartItem> items { get; set; } = new Dictionary<long, CartItem>();

        public DateTime? createdAt { get; set; }

        public string instanceId { get; set; } = "";

        // to return
        public List<ProductStatus>? divergencies { get; set; }

        // for dapr
        public Cart() { }

        public Cart(string customerId) { this.customerId = customerId; }

    }
}

