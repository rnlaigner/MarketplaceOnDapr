using System;
using System.Xml;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using ProductMS.Models;

namespace ProductMS.Infra
{
	public class ProductDbContext : DbContext
    {

        public DbSet<ProductModel> Products => Set<ProductModel>();

        public ProductDbContext()
		{
		}

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            options.UseNpgsql(@"Host=localhost;Port=5432;Database=product;Username=postgres;Password=password")
                .UseLoggerFactory(
                    LoggerFactory.Create(
                        b => b
                            .AddConsole()
                            .AddFilter(level => level >= LogLevel.Information)))
                .EnableSensitiveDataLogging()
                .EnableDetailedErrors();

            options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ProductModel>().ToTable(t => t.HasCheckConstraint(
                "CK_Product_Price", "price >= 0"
                ));

        }

    }
}

