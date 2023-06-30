using Microsoft.EntityFrameworkCore;
using SellerMS.Infra;
using SellerMS.Repositories;
using SellerMS.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDaprClient();

builder.Services.AddDbContextFactory<SellerDbContext>();
builder.Services.AddScoped<ISellerRepository, SellerRepository>();
builder.Services.AddScoped<ISellerService, SellerService>();

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
    var context = services.GetRequiredService<SellerDbContext>();
    RelationalDatabaseFacadeExtensions.Migrate(context.Database);

    context.Database.ExecuteSqlRaw(SellerDbContext.OrderSellerViewSql);
    context.Database.ExecuteSqlRaw(SellerDbContext.OrderSellerViewSqlIndex);
}

app.MapControllers();

app.MapHealthChecks("/health");

app.MapSubscribeHandler();

app.Run();