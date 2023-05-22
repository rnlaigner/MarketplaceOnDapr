using System;
using Common.Entities;

namespace Common.Events
{
    // an invoice is a request for payment
    public record InvoiceIssued
    (
        CustomerCheckout customer,
        long order_id,
        string invoice_number,
        decimal total_amount,
        IList<OrderItem> items,
        string instanceId
    );
}

