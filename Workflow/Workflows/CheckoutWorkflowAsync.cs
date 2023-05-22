using System;
using System.Reactive;
using System.Threading.Tasks;
using Workflow.Handlers;
using Common.Events;
using Common.Entities;
using Dapr.Workflow;
using Microsoft.Extensions.Logging;

namespace Workflows
{
    /*
     * Instead of HTTP APIs, the workflow always starts (and restarts)
     * through asynchronous events.
     */
    class CheckoutWorkflowAsync : Workflow<CustomerCheckout, CheckoutResult>
    {

        readonly ILogger logger;

        public CheckoutWorkflowAsync()
        {
            this.logger = new LoggerFactory().CreateLogger<CheckoutWorkflow>();
        }

        public override async Task<CheckoutResult> RunAsync(WorkflowContext context, CustomerCheckout customerCheckout)
        {

            string instanceId = context.InstanceId;
            // https://docs.dapr.io/developing-applications/building-blocks/workflow/workflow-features-concepts/#workflow-determinism-and-code-restraints
            var now = context.CurrentUtcDateTime;

            this.logger.LogInformation("Starting new workflow {0}:{1}", instanceId, now);

            CheckoutNotification checkoutNotification = new CheckoutNotification(customerCheckout.CustomerId, instanceId);

            Task<Cart> notifyTask = context.CallActivityAsync<Cart>(
                nameof(NotifyCheckoutActivity),
                checkoutNotification);

            this.logger.LogInformation("Activity notify checkout has been called");

            await notifyTask;

            // context.WaitForExternalEventAsync

            if (!notifyTask.IsCompletedSuccessfully || notifyTask.Result == null)
            {
                Console.WriteLine("Not completed successfully");
                return new CheckoutResult(Processed: false);
            }

            var cart = notifyTask.Result;

            // sending the instanceId so order service can ensure idempotence
            var checkout = new StockConfirmed(now, customerCheckout, cart.items.Select(c => c.Value).ToList(), instanceId);

            // TODO i believe the workflow should reserve all items before sending to order
            // and the same if payment fails...
            // to code a call to the stock and only after confirm all the stock, then I proceed withthe order procesisng

            Task<InvoiceIssued> invoiceIssuedTask = context.CallActivityAsync<InvoiceIssued>(
                nameof(StockConfirmed),
                checkout);

            this.logger.LogInformation("Activity process checkout has been called");

            InvoiceIssued InvoiceIssued = invoiceIssuedTask.Result;

            return new CheckoutResult(Processed: true);
        }
    }
}

