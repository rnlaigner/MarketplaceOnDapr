using System.Security.Authentication;
using CartMS.Repositories;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

var appName = "Cart";

// Add services to the container

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = $"MarketplaceDapr - {appName}", Version = "v1" });

});

builder.Services.AddSingleton<ICartRepository, CartRepository>();

builder.Services.AddDaprClient();
builder.Services.AddControllers(); //.AddDapr();

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", $"{appName} V1");
});

/*
// dont actually know the diff between the following config commands and the used ones in this project
app.UseRouting();
app.UseEndpoints(endpoints => {
    endpoints.MapSubscribeHandler();
    endpoints.MapControllers();
});
*/

// why a default route is needed?
// app.MapDefaultControllerRoute();

app.MapControllers();

// not needed unless using pub/sub
app.MapSubscribeHandler();

app.Run();

