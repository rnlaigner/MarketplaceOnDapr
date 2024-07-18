using Microsoft.EntityFrameworkCore;
using SellerMS.Infra;
using SellerMS.Repositories;
using SellerMS.Services;
using MysticMind.PostgresEmbed;
using Common.Utils;
using System.Runtime.InteropServices;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOptions();

IConfigurationSection configSection = builder.Configuration.GetSection("SellerConfig");
builder.Services.Configure<SellerConfig>(configSection);
var config = configSection.Get<SellerConfig>();
if (config == null)
    Environment.Exit(1);

Task? waitPgSql = null;
if (config.PostgresEmbed)
{
    PgServer server;
    var instanceId = Utils.GetGuid("SellerDb");
    var serverParams = new Dictionary<string, string>
    {
        { "synchronous_commit", "off" },
        { "max_connections", "10000" },
        { "listen_addresses", "*" }
    };
    if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
    {
        serverParams.Add( "unix_socket_directories", "/tmp");
        serverParams.Add( "unix_socket_group", "" );
        serverParams.Add( "unix_socket_permissions", "0777");
    }
    // serverParams.Add("shared_buffers", X);
    server = new PgServer("15.3.0", port: 5436, pgServerParams: serverParams, instanceId: instanceId);
    waitPgSql = server.StartAsync();
}

if(config.InMemoryDb){
    builder.Services.AddSingleton<ISellerRepository, InMemorySellerRepository>();
} else {
    builder.Services.AddDbContextFactory<SellerDbContext>();
    builder.Services.AddScoped<ISellerRepository, SellerRepository>();
}

builder.Services.AddScoped<ISellerService, SellerService>();

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

    if(!config.InMemoryDb){
        var context = services.GetRequiredService<SellerDbContext>();
        context.Database.Migrate();

        if (config.Unlogged)
        {
            var tableNames = context.Model.GetEntityTypes()
                                .Select(t => t.GetTableName())
                                .Distinct()
                                .ToList();
            foreach (var table in tableNames)
            {
                if(table is null || table.SequenceEqual("")) continue;
                context.Database.ExecuteSqlRaw($"ALTER TABLE seller.{table} SET unlogged");
            }
        }

        context.Database.ExecuteSqlRaw(SellerDbContext.ORDER_SELLER_VIEW_SQL);
        context.Database.ExecuteSqlRaw(SellerDbContext.ORDER_SELLER_VIEW_SQL_INDEX);
    }
}

app.UseCloudEvents();

app.MapControllers();

app.MapHealthChecks("/health");

app.MapSubscribeHandler();

app.Run();
