using CartMS.Infra;
using CartMS.Repositories;
using CartMS.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddDaprClient();

builder.Services.AddSingleton<ICartRepository, CartRepository>();
builder.Services.AddSingleton<IProductRepository, ProductRepository>();

builder.Services.AddSingleton<ICartService, CartService>();

builder.Services.AddControllers();

// https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/health-checks?view=aspnetcore-6.0
builder.Services.AddHealthChecks();

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

app.MapHealthChecks("/health");

// not needed unless using pub/sub
app.MapSubscribeHandler();

app.Run();