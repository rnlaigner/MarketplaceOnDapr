using CartMS.Infra;
using CartMS.Repositories;
using CartMS.Services;
using Microsoft.EntityFrameworkCore;
using MysticMind.PostgresEmbed;
using Common.Utils;
using System.Runtime.InteropServices;
using CartMS.Repositories.Impl;

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
    var serverParams = new Dictionary<string, string>
    {
        { "synchronous_commit", "off" },
        { "max_connections", "10000" },
        { "listen_addresses", "*" },
        { "shared_buffers", "3GB" },
        { "work_mem", "128MB" }
    };
    
    if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
    {
        serverParams.Add( "unix_socket_directories", "/tmp");
        serverParams.Add( "unix_socket_group", "" );
        serverParams.Add( "unix_socket_permissions", "0777");
    }
    server = new PgServer("15.3.0", port: 5431, pgServerParams: serverParams, instanceId: instanceId);
    waitPgSql = server.StartAsync();
}

if (config.InMemoryDb)
{
    builder.Services.AddSingleton<ICartRepository, InMemoryCartRepository>();
    builder.Services.AddSingleton<IProductReplicaRepository, InMemoryProductReplicaRepository>();
} else
{
    builder.Services.AddDbContext<CartDbContext>();
    builder.Services.AddScoped<ICartRepository, CartRepository>();
    builder.Services.AddScoped<IProductReplicaRepository, ProductReplicaRepository>();
}

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

    if(!config.InMemoryDb){
        var context = services.GetRequiredService<CartDbContext>();
        context.Database.Migrate();

        if (!config.Logging)
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
}

app.MapControllers();

app.MapHealthChecks("/health");

// not needed unless using pub/sub
app.MapSubscribeHandler();

app.Run();
