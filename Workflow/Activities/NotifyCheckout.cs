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

namespace Workflow.Activities
{

    class NotifyCheckout : WorkflowActivity<CheckoutNotification, Cart>
    {
        readonly ILogger logger;

        private readonly HttpClient httpClient;
        private readonly DaprClient daprClient;

        // private readonly DaprClient daprClient;

        public NotifyCheckout(ILoggerFactory loggerFactory) //DaprClient daprClient
        {
            this.logger = loggerFactory.CreateLogger<NotifyCheckout>();
            // this.daprClient = daprClient;
            this.httpClient = new HttpClient();
            this.httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            // Adding app id as part of the header
            this.httpClient.DefaultRequestHeaders.Add("dapr-app-id", "cart");
            this.daprClient = new DaprClientBuilder().Build();
        }

        public override async Task<Cart> RunAsync(WorkflowActivityContext context, CheckoutNotification checkoutNotification)
        {
            //using var daprClient = new DaprClientBuilder().Build();
            /*
            this.logger.LogInformation("Building checkout notification to Cart [1]");

            // cannot enable mdns discovery on my mac due to KU firewall policies
            // https://forum.arduino.cc/t/allow-mdns-discovery-on-macos/1089144
            // need to call the app directly


            //var httpClient = DaprClient.CreateInvokeHttpClient();
            var payload = JsonConvert.SerializeObject(checkoutNotification);

            //var response = await httpClient.GetAsync("http://localhost:5001/" + checkoutNotification.customerId);
            // var response = await httpClient.PatchAsync("http://localhost:5001/"+ checkoutNotification.customerId+"/checkout", str);
            var str = HttpUtils.BuildPayload(payload);

            
            HttpResponseMessage response;
            */
            this.logger.LogInformation("Sending checkout notification to Cart [2]");
            Cart cart;

            //https://github.com/dapr/quickstarts/blob/master/service_invocation/csharp/http/checkout/Program.cs
            /*
            try
            {
                response = await httpClient.PatchAsync("http://localhost:3501/"
                    // + checkoutNotification.customerId + "/test",
                    + "/test/" + checkoutNotification.customerId,
                    str);

                // response.EnsureSuccessStatusCode();
                this.logger.LogError("status code received is: {0}: ", response.StatusCode);

                if (response.IsSuccessStatusCode)
                {
                    var stream = response.Content.ReadAsStream();
                    StreamReader reader = new StreamReader(stream);
                    string productRet = reader.ReadToEnd();
                    cart = JsonConvert.DeserializeObject<Cart>(productRet);
                    return cart;
                }

            } catch(Exception e)
            {
                this.logger.LogError("Cannot send message 1: {0}", e.Message);
            }
            */

            try
            {
                // this.logger.LogInformation(notification.Message);
                CancellationTokenSource source = new CancellationTokenSource();
                CancellationToken cancellationToken = source.Token;

                /*
                var result = daprClient.CreateInvokeMethodRequest(HttpMethod.Patch, "cart",
                    // "/" + checkoutNotification.customerId + 
                    "test/"+ checkoutNotification.customerId,
                    checkoutNotification);

                this.logger.LogInformation("Path {0}", result.RequestUri.AbsolutePath);

                this.logger.LogInformation("Sending checkout notification to Cart [2]");
                cart = await daprClient.InvokeMethodAsync<Cart>(result, cancellationToken);
                */

                var result = daprClient.CreateInvokeMethodRequest(HttpMethod.Patch, "cart", "Test", checkoutNotification.customerId);
                cart = await daprClient.InvokeMethodAsync<Cart>(result);

                this.logger.LogInformation("Returning cart from checkout notification [3] {0}", cart.ToString());
                return cart;
            }
            catch (Exception e)
            {
                this.logger.LogError("Cannot send checkout notification: {0}", e.StackTrace);
                return null;
            }
            
        }
    }
}