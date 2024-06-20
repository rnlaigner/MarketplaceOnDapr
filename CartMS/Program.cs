using CartMS.Infra;
using CartMS.Repositories;
using CartMS.Services;
using Microsoft.EntityFrameworkCore;
using MysticMind.PostgresEmbed;
using Common.Utils;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOptions();

IConfigurationSection configSection = builder.Configuration.GetSection("CartConfig");
builder.Services.Configure<CartConfig>(configSection);
var config = configSection.Get<CartConfig>();
if (config == null)
    Environment.Exit(1);

Task? waitPgSql = null;
if (config.PostgresEmbed)
{
    PgServer server;
    var instanceId = Utils.GetGuid("CartDb");
    if (config.Unlogged)
    {
        var serverParams = new Dictionary<string, string>
        {
            { "synchronous_commit", "off" },
            { "max_connections", "300" },
            { "listen_addresses", "*" }
        };
        // serverParams.Add("shared_buffers", X);
        server = new PgServer("15.3.0", port: 5431, pgServerParams: serverParams, instanceId: instanceId);
    }
    else
    {
        server = new PgServer("15.3.0", port: 5431, instanceId: instanceId);
    }
    waitPgSql = server.StartAsync();
}

builder.Services.AddDbContext<CartDbContext>();

builder.Services.AddScoped<ICartRepository, CartRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<ICartService, CartService>();

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

    var context = services.GetRequiredService<CartDbContext>();
    context.Database.Migrate();

    if (config.Unlogged)
    {
        var tableNames = context.Model.GetEntityTypes()
                            .Select(t => t.GetTableName())
                            .Distinct()
                            .ToList();
        foreach (var table in tableNames)
        {
            context.Database.ExecuteSqlRaw($"ALTER TABLE cart.{table} SET unlogged");
        }
    }
}

app.MapControllers();

app.MapHealthChecks("/health");

// not needed unless using pub/sub
app.MapSubscribeHandler();

app.Run();