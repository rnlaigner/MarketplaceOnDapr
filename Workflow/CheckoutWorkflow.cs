using System;
using System.Reactive;
using System.Threading.Tasks;
using Workflow.Handlers;
using Common.Events;
using Common.Entities;
using Dapr.Workflow;
using Microsoft.Extensions.Logging;

namespace Workflow
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

        public override async Task<CheckoutResult> RunAsync(WorkflowContext context, CustomerCheckout customerCheckout)
        {

            string instanceId = context.InstanceId;
            // https://docs.dapr.io/developing-applications/building-blocks/workflow/workflow-features-concepts/#workflow-determinism-and-code-restraints
            var now = context.CurrentUtcDateTime;

            this.logger.LogInformation("Starting new workflow {0}:{1}", instanceId, now);

            CheckoutNotification checkoutNotification = new CheckoutNotification(customerCheckout.CustomerId, instanceId);

            Task<Cart> notifyTask = context.CallActivityAsync<Cart>(
                nameof(NotifyCheckout),
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
            Checkout checkout = new Checkout(now, customerCheckout, cart.items, instanceId);

            // TODO i believe the workflow should reserve all items before sending to order
            // and the same if payment fails...
            // to code a call to the stock and only after confirm all the stock, then I proceed withthe order procesisng

            Task<Invoice> invoiceTask = context.CallActivityAsync<Invoice>(
                nameof(ProcessCheckout),
                checkout);

            this.logger.LogInformation("Activity process checkout has been called");

            Invoice invoice = invoiceTask.Result;
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

