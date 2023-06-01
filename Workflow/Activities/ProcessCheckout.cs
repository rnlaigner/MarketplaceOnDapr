using System;
using System.Threading.Tasks;
using Common.Events;
using Common.Entities;
using Dapr.Workflow;
using Microsoft.Extensions.Logging;
using Dapr.Client;

namespace Workflow.Activities
{

    class ProcessCheckout : WorkflowActivity<StockConfirmed, InvoiceIssued>
    {
        readonly ILogger logger;

        // readonly DaprClient daprClient;

        public ProcessCheckout(ILoggerFactory loggerFactory) //, DaprClient daprClient)
        {
            this.logger = loggerFactory.CreateLogger<ProcessCheckout>();
            // this.daprClient = daprClient;
        }

        public override async Task<InvoiceIssued> RunAsync(WorkflowActivityContext context, StockConfirmed checkout)
        {

            this.logger.LogInformation("Process checkout has been called!");

            var daprClient = new DaprClientBuilder().Build();

            // this.logger.LogInformation(notification.Message);
            CancellationTokenSource source = new CancellationTokenSource();
            CancellationToken cancellationToken = source.Token;

            // better to make two dapr versions. one workflow with service invocation
            // and another with event-based approach (handling of errors in the app)
            // await daprClient.PublishEventAsync(PUBSUB_NAME, TOPIC_NAME, checkout, cancellationToken);

            // can also make it work with completion source
            // var cs = new TaskCompletionSource();cs.SetResult();

            // https://docs.dapr.io/developing-applications/building-blocks/service-invocation/howto-invoke-discover-services/
            // https://docs.dapr.io/getting-started/quickstarts/serviceinvocation-quickstart/
            // var message = daprClient.CreateInvokeMethodRequest(HttpMethod.Post, "cart", "checkout", customerCheckout);
            InvoiceIssued invoice = await daprClient.InvokeMethodAsync<Common.Events.StockConfirmed, InvoiceIssued>(
                HttpMethod.Post, "order", "checkout", checkout, cancellationToken);

            return invoice;
        }
    }
}