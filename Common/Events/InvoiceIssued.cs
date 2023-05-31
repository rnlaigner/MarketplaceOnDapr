using System;
using Common.Entities;

namespace Common.Events
{
    // an invoice is a request for payment
    public record InvoiceIssued
    (
        CustomerCheckout customer,
        long orderId,
        string invoiceNumber,
        DateTime issueDate,
        decimal totalAmount,
        IList<OrderItem> items,
        string instanceId = ""
    );
}

