using System;
using System.Threading.Tasks;
using Common.Events;
using Common.Entities;
using Dapr.Workflow;
using Microsoft.Extensions.Logging;
using System.Threading;
using Dapr.Client;
using System.Net.Http.Json;
using System.Text;
using Newtonsoft.Json;
using static Google.Rpc.Context.AttributeContext.Types;
using Workflow.Infra;

namespace Workflow.Handlers
{

    class NotifyCheckoutAsync : WorkflowActivity<CustomerCheckout, Cart>
    {
        private readonly ILogger logger;
        private readonly DaprClient daprClient;

        // private readonly DaprClient daprClient;

        public NotifyCheckoutAsync(ILoggerFactory loggerFactory) //DaprClient daprClient
        {
            this.logger = loggerFactory.CreateLogger<NotifyCheckoutAsync>();
            this.daprClient = new DaprClientBuilder().Build();
        }

        public override async Task<Cart> RunAsync(WorkflowActivityContext context, CustomerCheckout customerCheckout)
        {
            this.logger.LogInformation("Sending customer checkout to Cart [1]");
            await daprClient.PublishEventAsync("pubsub", nameof(CustomerCheckout), customerCheckout);
            return new Cart();
        }
    }
}