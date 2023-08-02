using Microsoft.EntityFrameworkCore;
using CartMS.Models;

namespace CartMS.Infra
{
	public class CartDbContext : DbContext
    {
        public DbSet<CartModel> Carts => Set<CartModel>();
        public DbSet<CartItemModel> CartItems => Set<CartItemModel>();
        public DbSet<ProductModel> Products => Set<ProductModel>();

        private readonly IConfiguration configuration;

        public CartDbContext(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            options.UseNpgsql(configuration.GetConnectionString("Database"))
                .EnableSensitiveDataLogging()
                .EnableDetailedErrors()
                .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("cart");
            modelBuilder.Entity<CartModel>()
                 .Property(e => e.status)
                 .HasConversion<string>();
        }

    }
}

