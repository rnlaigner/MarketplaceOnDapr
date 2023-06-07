using Google.Api;
using Microsoft.AspNetCore.Mvc;
using ProductMS.Infra;
using ProductMS.Repositories;
using ProductMS.Services;

var builder = WebApplication.CreateBuilder(args);

// https://docs.dapr.io/developing-applications/building-blocks/state-management/howto-share-state/

// Add functionality to inject IOptions<T>
builder.Services.AddOptions();

// Add our Config object so it can be injected
IConfigurationSection configSection = builder.Configuration.GetSection("ProductConfig");
builder.Services.Configure<ProductConfig>(configSection);

builder.Services.AddDaprClient();

// Add services to the container
builder.Services.AddDbContext<ProductDbContext>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IProductService, ProductService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHealthChecks();

/*
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    // options.SuppressModelStateInvalidFilter = true;
    options.InvalidModelStateResponseFactory = actionContext =>
    {
        // Do what you need here for specific cases with `actionContext` 
        // I believe you can cehck the action attributes 
        // if you'd like to make mark / handle specific cases by action attributes. 

        return new BadRequestObjectResult(actionContext.ModelState);
    };
});
*/

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
    var context = services.GetRequiredService<ProductDbContext>();
    context.Database.EnsureDeleted();
    context.Database.EnsureCreated();
}

// Configure the HTTP request pipeline.
app.MapControllers();

app.MapHealthChecks("/health");

app.MapSubscribeHandler();

app.Run();