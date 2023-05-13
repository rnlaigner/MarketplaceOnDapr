using Castle.DynamicProxy;
using Google.Api;
using ShipmentMS.Infra;
using ShipmentMS.Repositories;
using ShipmentMS.Service;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddDbContext<ShipmentDbContext>();

builder.Services.AddScoped<IShipmentRepository, ShipmentRepository>();
builder.Services.AddScoped<IPackageRepository, PackageRepository>();

builder.Services.AddDaprClient();
builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<TransactionalInterceptor>(); // Register the interceptor
builder.Services.AddSingleton<ProxyGenerator>(); // Register the proxy generator

// Configure the proxy generation for classes marked with the [Transactional] attribute
builder.Services.AddScoped<IShipmentService>(provider =>
{
    var interceptor = provider.GetRequiredService<TransactionalInterceptor>();
    var generator = provider.GetRequiredService<ProxyGenerator>();
    var dbContext = provider.GetRequiredService<ShipmentDbContext>();
    var shipRepo = provider.GetRequiredService<IShipmentRepository>();
    var packRepo = provider.GetRequiredService<IPackageRepository>();

    var loggerfactory = provider.GetRequiredService<ILoggerFactory>();
    var logger = loggerfactory.CreateLogger<ShipmentService>();

    var myClass = new ShipmentService(shipRepo, packRepo, logger); // Instantiate the class directly

    // Generate the proxy
    return generator.CreateInterfaceProxyWithTargetInterface(myClass, interceptor);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    var context = services.GetRequiredService<ShipmentDbContext>();
    context.Database.EnsureCreated();
}

app.MapControllers();
app.MapSubscribeHandler();

app.Run();

