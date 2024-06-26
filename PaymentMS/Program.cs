﻿using Microsoft.EntityFrameworkCore;
using PaymentMS.Infra;
using PaymentMS.Repositories;
using PaymentMS.Services;

var builder = WebApplication.CreateBuilder(args);

IConfigurationSection configSection = builder.Configuration.GetSection("PaymentConfig");
builder.Services.Configure<PaymentConfig>(configSection);

builder.Services.AddDaprClient();

builder.Services.AddSingleton<HttpClient>();
builder.Services.AddDbContext<PaymentDbContext>();
builder.Services.AddScoped<IExternalProvider, ExternalProviderProxy>();
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
builder.Services.AddScoped<IPaymentService, PaymentService>();

// Add services to the container.

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
    var context = services.GetRequiredService<PaymentDbContext>();
    context.Database.Migrate();
}

app.MapControllers();

app.MapHealthChecks("/health");

app.MapSubscribeHandler();

app.Run();