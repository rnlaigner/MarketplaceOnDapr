using System;
using System.Collections.Generic;
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
using Common.Utils;

var builder = WebApplication.CreateBuilder(args);

IConfigurationSection configSection = builder.Configuration.GetSection("OrderConfig");
builder.Services.Configure<OrderConfig>(configSection);
var config = configSection.Get<OrderConfig>();
if(config == null)
    System.Environment.Exit(1);

if (config.PostgresEmbed)
{
    PgServer server;
    var instanceId = Utils.GetGuid("OrderDb");
    if (config.Unlogged)
    {
        // https://www.postgresql.org/docs/current/config-setting.html#CONFIG-SETTING-NAMES-VALUES
        var serverParams = new Dictionary<string, string>();
        // switch off synchronous commit
        serverParams.Add("synchronous_commit", "off");
        // set max connections
        serverParams.Add("max_connections", "300");
        // The default value is localhost, which allows only local TCP/IP "loopback" connections to be made.
        serverParams.Add("listen_addresses", "*");
        // serverParams.Add("shared_buffers", X);
        server = new PgServer("15.3.0", port: 5432, pgServerParams: serverParams, instanceId: instanceId);
    }
    else
    {
        server = new PgServer("15.3.0", port: 5432, instanceId: instanceId);
    }
    server.Start();
}

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

// https://www.npgsql.org/doc/types/datetime.html
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<OrderDbContext>();
    try
    {
        Console.WriteLine(context.ConnectionString);
        context.Database.Migrate();
    }
    catch (Exception ex) { 
        Console.Write(ex.Message);
        throw new ApplicationException(ex.ToString());
    }

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

if(config.Streaming)
    app.UseCloudEvents();

app.MapControllers();

app.MapHealthChecks("/health");

if(config.Streaming)
    app.MapSubscribeHandler();

app.Run();