using StockMS.Repositories;
using StockMS.Infra;
using StockMS.Services;
using Microsoft.EntityFrameworkCore;
using MysticMind.PostgresEmbed;
using Common.Utils;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOptions();

// Add our Config object so it can be injected
IConfigurationSection configSection = builder.Configuration.GetSection("StockConfig");
builder.Services.Configure<StockConfig>(configSection);
var config = configSection.Get<StockConfig>();
if (config == null)
    System.Environment.Exit(1);

builder.Services.AddDbContext<StockDbContext>();

builder.Services.AddScoped<IStockRepository, StockRepository>();
builder.Services.AddScoped<IStockService, StockService>();

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
        PgServer server;
        var instanceId = Utils.GetGuid("StockDb");
        if (config.Unlogged)
        {
            var serverParams = new Dictionary<string, string>();
            serverParams.Add("synchronous_commit", "off");
            // serverParams.Add("shared_buffers", X);
            server = new PgServer("15.3.0", port: 5432, pgServerParams: serverParams);
        }
        else
        {
            server = new PgServer("15.3.0", port: 5432, instanceId: instanceId);
        }
        server.Start();
    }

    var context = services.GetRequiredService<StockDbContext>();
    context.Database.Migrate();

    if (config.Unlogged)
    {
        var tableNames = context.Model.GetEntityTypes()
                            .Select(t => t.GetTableName())
                            .Distinct()
                            .ToList();
        foreach (var table in tableNames)
        {
            context.Database.ExecuteSqlRaw($"ALTER TABLE stock.{table} SET unlogged");
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