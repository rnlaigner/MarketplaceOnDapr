using System;
using System.Security.Authentication;
using Google.Api;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using OrderMS.Common.Models;
using OrderMS.Common.Repositories;
using OrderMS.Handlers;
using OrderMS.Infra;
using OrderMS.Repositories;

var builder = WebApplication.CreateBuilder(args);

var appName = "Order";

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = $"MarketplaceDapr - {appName}", Version = "v1" });

});

// scoped here because db context is scoped
builder.Services.AddScoped<IOrderRepository, OrderRepository>();

builder.Services.AddDbContext<OrderDbContext>();

// https://learn.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection?view=aspnetcore-7.0
//OrderDbContext dbCtx = (OrderDbContext) builder.Services.BuildServiceProvider().GetRequiredService(typeof(OrderDbContext));
//builder.Services.AddSingleton<OrderEventHandler>(sp => new OrderEventHandler(dbCtx));

builder.Services.AddDaprClient();
builder.Services.AddControllers();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", $"{appName} V1");
});

// https://www.npgsql.org/doc/types/datetime.html
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    var context = services.GetRequiredService<OrderDbContext>();
    // context.Database.EnsureDeleted();
    context.Database.EnsureCreated();
    // DbInitializer.Initialize(context);

    /*
    var order = context.Orders.Where(c => c.id == 1).First();
    if (order is not null) context.Orders.Remove(order);
    */

    context.Orders.Add(new OrderModel()
    {
        // id = 1,
        customer_id = "1",
        // status = Common.Entities.OrderStatus.CREATED,
        purchase_date = DateTime.Now,
        count_items = 0,
        instanceId = "test"
    });
    context.SaveChanges();
}

app.MapControllers();
app.MapSubscribeHandler();

app.Run();

