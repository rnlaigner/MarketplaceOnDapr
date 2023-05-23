using System;
namespace Common.Entities
{
    /**
     * https://dev.olist.com/docs/orders
     */
    public enum OrderStatus
	{
        CREATED,
        PROCESSING,
        APPROVED,
        CANCELED,
        UNAVAILABLE,
        INVOICED,
        // generic term to address the order is on the way to the customer. fine grained tracking is provided by the shipment service
        IN_TRANSIT,
        DELIVERED,

        PAYMENT_FAILED,
        PAYMENT_PROCESSED
    }
}

