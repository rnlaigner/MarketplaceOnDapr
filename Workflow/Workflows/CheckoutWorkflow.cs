using System;
using System.Reactive;
using System.Threading.Tasks;
using Workflow.Activities;
using Common.Events;
using Common.Entities;
using Dapr.Workflow;
using Microsoft.Extensions.Logging;

namespace Workflows
{

    record CheckoutResult(bool Processed);

    class CheckoutWorkflow : Workflow<CustomerCheckout, CheckoutResult>
    {

        readonly ILogger logger;

        /*
         * must be a non-abstract type with a public parameterless constructor 
         * in order to use it as parameter 'TWorkflow' in the generic type or 
         * method 'WorkflowRuntimeOptions.RegisterWorkflow<TWorkflow>()
         */
        public CheckoutWorkflow()
        {
            this.logger = new LoggerFactory().CreateLogger<CheckoutWorkflow>();
        }

        /**
         * https://docs.dapr.io/developing-applications/building-blocks/workflow/workflow-patterns/#async-http-apis
         */
        public override async Task<CheckoutResult> RunAsync(WorkflowContext context, CustomerCheckout customerCheckout)
        {

            string instanceId = context.InstanceId;
            // https://docs.dapr.io/developing-applications/building-blocks/workflow/workflow-features-concepts/#workflow-determinism-and-code-restraints
            var now = context.CurrentUtcDateTime;

            this.logger.LogInformation("Starting new workflow {0} at {1}", instanceId, now);

            CheckoutNotification checkoutNotification = new CheckoutNotification(customerCheckout.CustomerId, instanceId);

            Task<Cart> notifyTask = context.CallActivityAsync<Cart>(
                nameof(NotifyCheckout),
                checkoutNotification);

            this.logger.LogInformation("Activity notify checkout has been called");

            await notifyTask;

            if (!notifyTask.IsCompletedSuccessfully || notifyTask.Result == null)
            {
                Console.WriteLine("Not completed successfully");
                return new CheckoutResult(Processed: false);
            }

            var cart = notifyTask.Result;

            // sending the instanceId so order service can ensure idempotence
            StockConfirmed checkout = new StockConfirmed(now, customerCheckout, cart.items.Select(c => c.Value).ToList(), instanceId);

            Task<InvoiceIssued> InvoiceIssuedTask = context.CallActivityAsync<InvoiceIssued>(
                nameof(StockConfirmed),
                checkout);

            this.logger.LogInformation("Activity process checkout has been called");

            InvoiceIssued InvoiceIssued = InvoiceIssuedTask.Result;
            /*
            if (!result.Success)
            {
                // End the workflow here since we don't have sufficient inventory
                context.SetCustomStatus($"Insufficient inventory for {order.Name}");
                return new OrderResult(Processed: false);
            }
            
            await context.CallActivityAsync(
                nameof(ProcessPaymentActivity),
                new PaymentRequest(RequestId: orderId, order.TotalCost, "USD"));

            await context.CallActivityAsync(
                nameof(NotifyActivity),
                new Notification($"Order {orderId} processed successfully!"));
            */
            // End the workflow with a success result
            return new CheckoutResult(Processed: true);
        }
    }
}

