using Microsoft.EntityFrameworkCore;
using SellerMS.Infra;
using SellerMS.Repositories;
using SellerMS.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<SellerDbContext>();
builder.Services.AddScoped<ISellerRepository, SellerRepository>();
builder.Services.AddScoped<ISellerService, SellerService>();


// Add services to the container.
builder.Services.AddDaprClient();
builder.Services.AddControllers();

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

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<SellerDbContext>();
    context.Database.EnsureDeleted();
    context.Database.EnsureCreated();

    context.Database.ExecuteSqlRaw(SellerDbContext.OrderViewSql);
    context.Database.ExecuteSqlRaw(SellerDbContext.OrderViewSqlIndex);

    context.Database.ExecuteSqlRaw(SellerDbContext.ShipmentViewSql);
    context.Database.ExecuteSqlRaw(SellerDbContext.ShipmentViewSqlIndex);
}

app.MapControllers();
app.MapSubscribeHandler();

app.Run();

