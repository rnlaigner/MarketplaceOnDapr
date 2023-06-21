using System;
using Common.Entities;
using Common.Integration;
using Common.Requests;

namespace Common.Events
{
    /*
     * "An invoice acts as a request for payment for the delivery of goods or services."
     * Source: https://invoice.2go.com/learn/invoices/invoice-vs-purchase-order/
     * An invoice data structure contains all necessary info for the payment 
     * actor to process a payment
     */
    public record InvoiceIssued
    (
        CustomerCheckout customer,
        long orderId,
        string invoiceNumber,
        DateTime issueDate,
        decimal totalInvoice,
        IList<OrderItem> items,
        int instanceId
    );
}

