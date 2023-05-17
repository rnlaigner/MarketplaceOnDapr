﻿using System.Security.Authentication;
using CartMS.Infra;
using CartMS.Repositories;
using Google.Api;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container

builder.Services.AddSingleton<ICartRepository, CartRepository>();
builder.Services.AddSingleton<IProductRepository, ProductRepository>();

builder.Services.AddDaprClient();
builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add functionality to inject IOptions<T>
builder.Services.AddOptions();

// Add our Config object so it can be injected
builder.Services.Configure<CartConfig>( builder.Configuration.GetSection("CartConfig") );

var app = builder.Build();

// Configure the HTTP request pipeline.

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

// not needed unless using pub/sub
app.MapSubscribeHandler();

app.Run();

