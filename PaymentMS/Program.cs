using PaymentMS.Infra;
using PaymentMS.Services;

var builder = WebApplication.CreateBuilder(args);

IConfigurationSection configSection = builder.Configuration.GetSection("PaymentConfig");
builder.Services.Configure<PaymentConfig>(configSection);

builder.Services.AddDaprClient();

builder.Services.AddDbContext<PaymentDbContext>();
builder.Services.AddScoped<IExternalProvider, MockExternalProvider>();
builder.Services.AddScoped<IPaymentService, PaymentService>();

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddHealthChecks();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    var context = services.GetRequiredService<PaymentDbContext>();
    context.Database.EnsureDeleted();
    context.Database.EnsureCreated();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCloudEvents();

app.MapControllers();

app.MapHealthChecks("/health");

app.MapSubscribeHandler();

app.Run();