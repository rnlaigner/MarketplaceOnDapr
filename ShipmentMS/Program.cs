using Dapr.Client;
using ShipmentMS.Infra;
using ShipmentMS.Repositories;
using ShipmentMS.Service;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOptions();
IConfigurationSection configSection = builder.Configuration.GetSection("ShipmentConfig");
builder.Services.Configure<ShipmentConfig>(configSection);


// Add services to the container.

builder.Services.AddDbContext<ShipmentDbContext>();

builder.Services.AddScoped<IShipmentRepository, ShipmentRepository>();
builder.Services.AddScoped<IPackageRepository, PackageRepository>();

builder.Services.AddScoped<IShipmentService, ShipmentService>();

builder.Services.AddDaprClient();
builder.Services.AddControllers();
builder.Services.AddHealthChecks();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
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

    var context = services.GetRequiredService<ShipmentDbContext>();
    context.Database.EnsureDeleted();
    context.Database.EnsureCreated();
}

app.MapControllers();

app.MapHealthChecks("/health");

app.MapSubscribeHandler();

app.Run();