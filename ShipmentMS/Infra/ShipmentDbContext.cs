using System;
using Microsoft.EntityFrameworkCore;
using ShipmentMS.Models;

namespace ShipmentMS.Infra
{
	public class ShipmentDbContext : DbContext
	{

        public DbSet<ShipmentModel> Shipments => Set<ShipmentModel>();
        public DbSet<PackageModel> Packages => Set<PackageModel>();

        public ShipmentDbContext()
		{
		}

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            options.UseNpgsql(@"Host=localhost;Port=5432;Database=shipment;Username=postgres;Password=password")
                .UseLoggerFactory(
                    LoggerFactory.Create(
                        b => b
                            .AddConsole()
                            .AddFilter(level => level >= LogLevel.Information)))
                .EnableSensitiveDataLogging()
                .EnableDetailedErrors();

            options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
        }

        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
            configurationBuilder.Properties<decimal>()
                .HaveColumnType("decimal")
                .HavePrecision(4, 2);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ShipmentModel>()
                           .Property(e => e.status)
                           .HasConversion<string>();

            modelBuilder.Entity<PackageModel>()
                           .Property(e => e.status)
                           .HasConversion<string>();

        }

    }
}