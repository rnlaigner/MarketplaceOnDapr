using System;
using System.Text.Json.Serialization;
using Workflow.Handlers;
using Common.Entities;
using Dapr.Client;
using Dapr.Workflow;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Workflows;
using JsonOptions = Microsoft.AspNetCore.Http.Json.JsonOptions;
using Microsoft.Extensions.Hosting;

// The workflow host is a background service that connects to the sidecar over gRPC
WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Configure HTTP JSON options.
builder.Services.Configure<JsonOptions>(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
    options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
});

// Dapr workflows are registered as part of the service configuration
builder.Services.AddDaprWorkflow(options =>
{
    // Note that it's also possible to register a lambda function as the workflow
    // or activity implementation instead of a class.
    options.RegisterWorkflow<CheckoutWorkflow>();

    // These are the activities that get invoked by the workflow(s).
    options.RegisterActivity<NotifyCheckoutActivity>();
    options.RegisterActivity<ProcessCheckoutActivity>();
    // options.RegisterActivity<ProcessPaymentActivity>();
});

WebApplication app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

// POST starts new checkout workflow instance
app.MapPost("/carts", async (WorkflowEngineClient client, [FromBody] CustomerCheckout customerCheckout) =>
{
    // Randomly generated order ID that is 8 characters long.
    string workflowId = Guid.NewGuid().ToString()[..8];
    await client.ScheduleNewWorkflowAsync(nameof(CheckoutWorkflow), workflowId, customerCheckout, DateTime.Now);

    // return an HTTP 202 and a Location header to be used for status query
    return Results.AcceptedAtRoute("GetCheckoutInfoEndpoint", new { workflowId });
});

app.MapGet("/test", async (WorkflowEngineClient client) =>
{
    // Console.WriteLine("starting workflow call!");
    CustomerCheckout customerCheckout = new CustomerCheckout(
                "1",
                "",
                "",
                "",
                "",
                "",
                "",
                "",
                PaymentType.CREDIT_CARD.ToString(),
                "",
                "",
                "",
                "",
                "",
                 1,
                Array.Empty<decimal>()
            );

    // Randomly generated order ID that is 8 characters long.
    string transactionId = Guid.NewGuid().ToString()[..8];
    await client.ScheduleNewWorkflowAsync(nameof(CheckoutWorkflow), transactionId, customerCheckout, DateTime.Now);

    // return an HTTP 202 and a Location header to be used for status query
    // return Results.AcceptedAtRoute("GetCheckoutInfoEndpoint", new { transactionId });
    return Results.Accepted();
});

// app.Run();
await app.RunAsync();

/*
Task appTask = app.RunAsync().ContinueWith( async action =>
{
    Console.WriteLine("HTTP server started. Initiating cart item add");
    CartItem cartItem = new CartItem(
         1, // pId
         1, // seller
         10, // price
        0, // old price
        0, // 
        1 // qty
    );
    using var client = new DaprClientBuilder().Build();
    var result = client.CreateInvokeMethodRequest(HttpMethod.Patch, "cart", "0/add", cartItem);
    await client.InvokeMethodAsync(result);
    Console.WriteLine("Cart item added");
} );

// this does not work with dapr client
Console.WriteLine("\n *************************************************************************");
Console.WriteLine("              Workflow started. Configuration:          ");
Console.WriteLine("                Press any key to terminate...           ");
Console.WriteLine("\n *************************************************************************");
Console.ReadLine();
//await app.StopAsync();

appTask.GetAwaiter().GetResult();
*/