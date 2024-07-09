using CustomerMS.Infra;
using CustomerMS.Repositories;
using CustomerMS.Services;
using Microsoft.EntityFrameworkCore;
using MysticMind.PostgresEmbed;
using Common.Utils;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOptions();

IConfigurationSection configSection = builder.Configuration.GetSection("CustomerConfig");
builder.Services.Configure<CustomerConfig>(configSection);
var config = configSection.Get<CustomerConfig>();
if (config == null)
    Environment.Exit(1);

Task? waitPgSql = null;
if (config.PostgresEmbed)
{
    PgServer server;
    var instanceId = Utils.GetGuid("CustomerDb");
    if (config.Unlogged)
    {
        var serverParams = new Dictionary<string, string>
        {
            { "synchronous_commit", "off" },
            { "max_connections", "10000" },
            { "listen_addresses", "*" }
        };
        // serverParams.Add("shared_buffers", X);
        server = new PgServer("15.3.0", port: 5437, pgServerParams: serverParams, instanceId: instanceId);
    }
    else
    {
        server = new PgServer("15.3.0", port: 5437, instanceId: instanceId);
    }
    waitPgSql = server.StartAsync();
}

builder.Services.AddDbContext<CustomerDbContext>();

builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<ICustomerService, CustomerService>();

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

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    if (config.PostgresEmbed)
    {
        if (waitPgSql is not null) await waitPgSql;
        else
            throw new Exception("PostgreSQL was not setup correctly!");
    }

    var context = services.GetRequiredService<CustomerDbContext>();
    context.Database.Migrate();

    if (config.Unlogged)
    {
        var tableNames = context.Model.GetEntityTypes()
                            .Select(t => t.GetTableName())
                            .Distinct()
                            .ToList();
        foreach (var table in tableNames)
        {
            context.Database.ExecuteSqlRaw($"ALTER TABLE customer.{table} SET unlogged");
        }
    }
}

app.UseCloudEvents();

app.MapControllers();

app.MapHealthChecks("/health");

app.MapSubscribeHandler();

app.Run();