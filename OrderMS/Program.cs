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
using System.Threading.Tasks;

var builder = WebApplication.CreateBuilder(args);

// Add our Config object so it can be injected
IConfigurationSection configSection = builder.Configuration.GetSection("OrderConfig");
builder.Services.Configure<OrderConfig>(configSection);
var config = configSection.Get<OrderConfig>();
if(config == null)
    Environment.Exit(1);

Task? waitPgSql = null;
if (config.PostgresEmbed)
{
    PgServer server;
    var instanceId = Utils.GetGuid("OrderDb");
    if (config.Unlogged)
    {
        // https://www.postgresql.org/docs/current/config-setting.html#CONFIG-SETTING-NAMES-VALUES
        var serverParams = new Dictionary<string, string>
        {
            // switch off synchronous commit
            { "synchronous_commit", "off" },
            // set max connections
            { "max_connections", "300" },
            // The default value is localhost, which allows only local TCP/IP "loopback" connections to be made.
            { "listen_addresses", "*" }
        };
        // serverParams.Add("shared_buffers", X);
        server = new PgServer("15.3.0", port: 5432, pgServerParams: serverParams, instanceId: instanceId);
    }
    else
    {
        server = new PgServer("15.3.0", port: 5432, instanceId: instanceId);
    }
    waitPgSql = server.StartAsync();
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

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
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

    if (config.PostgresEmbed)
    {
        if (waitPgSql is not null) {
            Console.WriteLine("will wait for postgresql");
            await waitPgSql;
        }
        else {
            throw new Exception("PostgreSQL was not setup correctly!");
        }
    }

    var context = services.GetRequiredService<OrderDbContext>();
    try
    {
        // Console.WriteLine(context.ConnectionString);
        Console.WriteLine("will migrate");
        context.Database.Migrate();
  
        if (config.Unlogged)
        {
            Console.WriteLine("will set unlogged");
            var tableNames = context.Model.GetEntityTypes()
                                .Select(t => t.GetTableName())
                                .Distinct()
                                .ToList();
            foreach (var table in tableNames)
            {
                context.Database.ExecuteSqlRaw($"ALTER TABLE \"order\".{table} SET unlogged");
            }
        }
    }
    catch (Exception ex) { 
        Console.Write(ex.Message);
        throw new ApplicationException(ex.ToString());
    }

    // set ram disk
    // https://www.dbi-services.com/blog/can-i-put-my-temporary-tablespaces-on-a-ram-disk-with-postgresql/
    /*
    if (!config.RamDiskDir.Equals(""))
    {
        context.Database.ExecuteSqlRaw($"CREATE TABLESPACE my_tbs LOCATION '{config.RamDiskDir}'");
        context.Database.ExecuteSqlRaw($"SET default_tablespace = 'my_tbs'");
    }
    */
}
Console.WriteLine("DB block is passed");

if(config.Streaming)
    app.UseCloudEvents();

app.MapControllers();

app.MapHealthChecks("/health");

if(config.Streaming)
    app.MapSubscribeHandler();

app.Run();
