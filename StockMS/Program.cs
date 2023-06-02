using StockMS.Repositories;
using Microsoft.OpenApi.Models;
using StockMS.Infra;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container

builder.Services.AddScoped<IStockRepository, StockRepository>();

builder.Services.AddDbContext<StockDbContext>();

builder.Services.AddDaprClient();
builder.Services.AddControllers();

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

    var context = services.GetRequiredService<StockDbContext>();
    context.Database.EnsureDeleted();
    context.Database.EnsureCreated();
}

// Configure the HTTP request pipeline.

app.MapControllers();
app.MapSubscribeHandler();

app.Run();