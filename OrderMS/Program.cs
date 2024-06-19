using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OrderMS.Infra;
using OrderMS.Common.Infra;
using OrderMS.Common.Repositories;
using OrderMS.Handlers;
using OrderMS.Repositories;
using OrderMS.Services;
using System.Linq;
using MysticMind.PostgresEmbed;

var builder = WebApplication.CreateBuilder(args);

IConfigurationSection configSection = builder.Configuration.GetSection("OrderConfig");
builder.Services.Configure<OrderConfig>(configSection);

// scoped here because db context is scoped
builder.Services.AddDbContext<OrderDbContext>();
// https://learn.microsoft.com/en-us/ef/core/performance/advanced-performance-topics?tabs=with-di%2Cexpression-api-with-constant#dbcontext-pooling
// builder.Services.AddDbContextPool<OrderDbContext>(o=>o.UseNpgsql);

builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IOrderService, OrderService>();

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

bool OrderStreaming = false;

// https://www.npgsql.org/doc/types/datetime.html
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var config = services.GetRequiredService<OrderConfig>();

    if (config.OrderStreaming)
    {
        OrderStreaming = true;
    }

    if (config.PostgresEmbed)
    {
        var server = new PgServer("15.3.0", port: 5432, clearInstanceDirOnStop: true);
        server.Start();
    }

    var context = services.GetRequiredService<OrderDbContext>();
    context.Database.Migrate();

    if (config.Unlogged)
    {
        var tableNames = context.Model.GetEntityTypes()
                            .Select(t => t.GetTableName())
                            .Distinct()
                            .ToList();
        foreach (var table in tableNames)
        {
            context.Database.ExecuteSqlRaw($"ALTER TABLE \"order\".{table} SET unlogged");
        }
    }

    // set ram disk
    // https://www.dbi-services.com/blog/can-i-put-my-temporary-tablespaces-on-a-ram-disk-with-postgresql/
    if (!config.RamDiskDir.Equals(""))
    {
        context.Database.ExecuteSqlRaw($"CREATE TABLESPACE my_tbs LOCATION '{config.RamDiskDir}'");
        context.Database.ExecuteSqlRaw($"SET default_tablespace = 'my_tbs'");
    }
}

if(OrderStreaming)
    app.UseCloudEvents();

app.MapControllers();

app.MapHealthChecks("/health");

if (OrderStreaming)
    app.MapSubscribeHandler();

app.Run();