using Microsoft.EntityFrameworkCore;
using PaymentMS.Infra;
using PaymentMS.Repositories;
using PaymentMS.Services;
using MysticMind.PostgresEmbed;
using Common.Utils;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOptions();

IConfigurationSection configSection = builder.Configuration.GetSection("PaymentConfig");
builder.Services.Configure<PaymentConfig>(configSection);
var config = configSection.Get<PaymentConfig>();
if (config == null)
    Environment.Exit(1);

Task? waitPgSql = null;
if (config.PostgresEmbed)
{
    PgServer server;
    var instanceId = Utils.GetGuid("PaymentDb");
    if (config.Unlogged)
    {
        var serverParams = new Dictionary<string, string>
        {
            { "synchronous_commit", "off" },
            { "max_connections", "300" },
            { "listen_addresses", "*" }
        };
        // serverParams.Add("shared_buffers", X);
        server = new PgServer("15.3.0", port: 5434, pgServerParams: serverParams, instanceId: instanceId);
    }
    else
    {
        server = new PgServer("15.3.0", port: 5434, instanceId: instanceId);
    }
    waitPgSql = server.StartAsync();
}

builder.Services.AddSingleton<HttpClient>();

builder.Services.AddDbContext<PaymentDbContext>();

builder.Services.AddScoped<IExternalProvider, ExternalProviderProxy>();
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
builder.Services.AddScoped<IPaymentService, PaymentService>();

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

    var context = services.GetRequiredService<PaymentDbContext>();
    context.Database.Migrate();

    if (config.Unlogged)
    {
        var tableNames = context.Model.GetEntityTypes()
                            .Select(t => t.GetTableName())
                            .Distinct()
                            .ToList();
        foreach (var table in tableNames)
        {
            context.Database.ExecuteSqlRaw($"ALTER TABLE payment.{table} SET unlogged");
        }
    }
}

if (config.Streaming)
    app.UseCloudEvents();

app.MapControllers();

app.MapHealthChecks("/health");

if (config.Streaming)
    app.MapSubscribeHandler();

app.Run();