using System;
using Microsoft.EntityFrameworkCore;
using SellerMS.Models;

namespace SellerMS.Infra
{
    public class SellerDbContext : DbContext
    {

        public DbSet<OrderEntryViewModel> OrderEntries => Set<OrderEntryViewModel>();
        public DbSet<OrderViewModel> Orders => Set<OrderViewModel>();

        public SellerDbContext()
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            options.UseNpgsql(@"Host=localhost;Port=5432;Database=seller;Username=postgres;Password=password")
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

        }
    }
}
