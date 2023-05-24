using ProductMS.Infra;
using ProductMS.Repositories;

var builder = WebApplication.CreateBuilder(args);

// https://docs.dapr.io/developing-applications/building-blocks/state-management/howto-share-state/
// default to shared state
// bool sharedState = Boolean.Parse(builder.Configuration["State:Shared"] ?? "true");

// Add functionality to inject IOptions<T>
builder.Services.AddOptions();

// Add our Config object so it can be injected
IConfigurationSection configSection = builder.Configuration.GetSection("ProductConfig");
builder.Services.Configure<ProductConfig>(configSection);

bool sharedState = bool.Parse(configSection["SharedState"] != null ? configSection["SharedState"] : "false");

// Add services to the container
if (sharedState)
{
    builder.Services.AddScoped<IProductRepository, DaprProductRepository>();
} else
{
    builder.Services.AddScoped<IProductRepository, SqlProductRepository>();
    builder.Services.AddDbContext<ProductDbContext>();
}

builder.Services.AddDaprClient();
builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (!sharedState)
{
    AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        var context = services.GetRequiredService<ProductDbContext>();
        // context.Database.EnsureDeleted();
        context.Database.EnsureCreated();
    }
}

// Configure the HTTP request pipeline.

app.MapControllers();
app.MapSubscribeHandler();

app.Run();