using System.Security.Authentication;
using StockMS.Repositories;
using Microsoft.OpenApi.Models;
using StockMS.Infra;

var builder = WebApplication.CreateBuilder(args);

var appName = "Stock";

// Add services to the container

/*
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = $"MarketplaceDapr - {appName}", Version = "v1" });

});
*/

builder.Services.AddScoped<IStockRepository, StockRepository>();

builder.Services.AddDbContext<StockDbContext>();

builder.Services.AddDaprClient();
builder.Services.AddControllers(); //.AddDapr();

var app = builder.Build();

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    var context = services.GetRequiredService<StockDbContext>();
    // context.Database.EnsureDeleted();
    context.Database.EnsureCreated();

    // context.SaveChanges();
}

// Configure the HTTP request pipeline.

/*
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", $"{appName} V1");
});
*/

app.MapControllers();

// not needed unless using pub/sub
app.MapSubscribeHandler();

app.Run();

