using System;
using Microsoft.EntityFrameworkCore;
using ShipmentMS.Models;

namespace ShipmentMS.Infra
{
	public class ShipmentDbContext : DbContext
	{

        public DbSet<ShipmentModel> Shipments => Set<ShipmentModel>();
        public DbSet<PackageModel> Packages => Set<PackageModel>();

        private readonly IConfiguration configuration;

        public ShipmentDbContext(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            options.UseNpgsql(configuration.GetConnectionString("Database"))
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