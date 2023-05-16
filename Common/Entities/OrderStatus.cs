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
        SHIPPED,
        DELIVERED,

        PAYMENT_FAILED,
        PAYMENT_PROCESSED
    }
}

