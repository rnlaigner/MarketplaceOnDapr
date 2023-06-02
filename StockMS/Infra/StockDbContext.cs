using System;
using System.Xml;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using StockMS.Models;

namespace StockMS.Infra
{
	public class StockDbContext : DbContext
    {

        public DbSet<StockItemModel> StockItems => Set<StockItemModel>();

        public StockDbContext()
		{
		}

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            options.UseNpgsql(@"Host=localhost;Port=5432;Database=stock;Username=postgres;Password=password")
                .AddInterceptors(new TransactionInterceptor()) // even with FOR UPDATE, serializable is important for idempotency? or not?
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
            // deprecated
            // https://stackoverflow.com/questions/13232777
            // modelBuilder.Entity<StockItemModel>(entity => entity.HasCheckConstraint("CK_StockItem_QtyAvailable", "[qty_available] >= 0");

            modelBuilder.Entity<StockItemModel>().ToTable(t => t.HasCheckConstraint(
                "CK_StockItem_QtyAvailable", "qty_available >= 0"
                ));

            modelBuilder.Entity<StockItemModel>().ToTable(t => t.HasCheckConstraint(
                "CK_StockItem_QtyReservedLessThanQtyAvailable", "qty_reserved <= qty_available"
                ));

            modelBuilder.Entity<StockItemModel>().ToTable(t => t.HasCheckConstraint(
                "CK_StockItem_QtyReserved", "qty_reserved >= 0"
                ));

        }

    }
}

