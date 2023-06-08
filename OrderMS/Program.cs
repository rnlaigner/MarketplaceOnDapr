using System;
using System.Security.Authentication;
using Google.Api;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using OrderMS.Common.Infra;
using OrderMS.Common.Models;
using OrderMS.Common.Repositories;
using OrderMS.Handlers;
using OrderMS.Infra;
using OrderMS.Repositories;
using OrderMS.Services;

var builder = WebApplication.CreateBuilder(args);

IConfigurationSection configSection = builder.Configuration.GetSection("OrderConfig");
builder.Services.Configure<OrderConfig>(configSection);

// scoped here because db context is scoped
builder.Services.AddDbContext<OrderDbContext>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IOrderService, OrderService>();

// https://learn.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection?view=aspnetcore-7.0
//OrderDbContext dbCtx = (OrderDbContext) builder.Services.BuildServiceProvider().GetRequiredService(typeof(OrderDbContext));
//builder.Services.AddSingleton<OrderEventHandler>(sp => new OrderEventHandler(dbCtx));

builder.Services.AddDaprClient();
builder.Services.AddControllers();

builder.Services.AddHealthChecks();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCloudEvents();

// https://www.npgsql.org/doc/types/datetime.html
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    var context = services.GetRequiredService<OrderDbContext>();
    context.Database.EnsureDeleted();
    context.Database.EnsureCreated();
}

app.MapControllers();

app.MapHealthChecks("/health");

app.MapSubscribeHandler();

app.Run();