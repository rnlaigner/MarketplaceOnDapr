using PaymentProvider.Infra;
using PaymentProvider.Services;

var builder = WebApplication.CreateBuilder(args);

IConfigurationSection configSection = builder.Configuration.GetSection("PaymentProviderConfig");
builder.Services.Configure<PaymentProviderConfig>(configSection);

// Add services to the container.
builder.Services.AddScoped<IPaymentProvider, PaymentProviderService>();

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

app.MapControllers();

app.MapHealthChecks("/health");

app.Run();