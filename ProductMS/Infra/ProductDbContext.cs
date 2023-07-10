using Microsoft.EntityFrameworkCore;
using ProductMS.Models;

namespace ProductMS.Infra
{
	public class ProductDbContext : DbContext
    {
        public DbSet<ProductModel> Products => Set<ProductModel>();

        private readonly IConfiguration configuration;

        public ProductDbContext(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            options.UseNpgsql(configuration.GetConnectionString("Database"))
                .EnableSensitiveDataLogging()
                .EnableDetailedErrors();

            options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
        }

        /*
        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
            configurationBuilder.Properties<decimal>()
                .HaveColumnType("decimal")
                .HavePrecision(4, 2);
        }
        */

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ProductModel>().ToTable(t => t.HasCheckConstraint(
                "CK_Product_Price", "price >= 0"
                ));

        }

    }
}

