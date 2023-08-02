using Microsoft.EntityFrameworkCore;
using StockMS.Models;

namespace StockMS.Infra
{
	public class StockDbContext : DbContext
    {

        public DbSet<StockItemModel> StockItems => Set<StockItemModel>();

        private readonly IConfiguration configuration;

        public StockDbContext(IConfiguration configuration)
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
            modelBuilder.HasDefaultSchema("stock");
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

