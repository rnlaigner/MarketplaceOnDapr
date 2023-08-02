using StockMS.Repositories;
using StockMS.Infra;
using StockMS.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOptions();

// Add our Config object so it can be injected
IConfigurationSection configSection = builder.Configuration.GetSection("StockConfig");
builder.Services.Configure<StockConfig>(configSection);

// Add services to the container
builder.Services.AddDaprClient();

builder.Services.AddDbContext<StockDbContext>();

builder.Services.AddScoped<IStockRepository, StockRepository>();
builder.Services.AddScoped<IStockService, StockService>();

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
    var context = services.GetRequiredService<StockDbContext>();
    context.Database.Migrate();
}

// Configure the HTTP request pipeline.
app.MapControllers();

app.MapHealthChecks("/health");

app.MapSubscribeHandler();

app.Run();