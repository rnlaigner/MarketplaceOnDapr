using System.Transactions;
using Microsoft.EntityFrameworkCore;
using SellerMS.Models;
using Laraue.EfCoreTriggers.PostgreSql.Extensions;
using Google.Api;
using Laraue.EfCoreTriggers.Common.Extensions;

namespace SellerMS.Infra
{
    public class SellerDbContext : DbContext
    {

        public DbSet<OrderEntryViewModel> OrderEntries => Set<OrderEntryViewModel>();
        public DbSet<OrderViewModel> OrderView => Set<OrderViewModel>();

        public DbSet<ShipmentViewModel> ShipmentView => Set<ShipmentViewModel>();
        public DbSet<ShipmentEntryViewModel> ShipmentEntries => Set<ShipmentEntryViewModel>();

        public DbSet<ProductEntryViewModel> ProductEntries => Set<ProductEntryViewModel>();

        public SellerDbContext()
        {
        }

        public const string OrderViewSql = $"CREATE MATERIALIZED VIEW IF NOT EXISTS {nameof(OrderViewModel)} AS SELECT seller_id, COUNT(count_items) as count_orders, SUM(total_items) as total_overall, SUM(total_amount) as revenue, AVG(total_items) as avg_order_value, AVG(total_amount) as avg_order_revenue FROM order_entry_view GROUP BY seller_id";
        public const string OrderViewSqlIndex = $"CREATE UNIQUE INDEX IF NOT EXISTS seller_index ON {nameof(OrderViewModel)} (seller_id)";

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            // https://github.com/win7user10/Laraue.EfCoreTriggers#laraueefcoretriggerspostgresql
            options = options
                .UseNpgsql(@"Host=localhost;Port=5432;Database=seller;Username=postgres;Password=password") //, x => x.MigrationsAssembly(typeof(SellerDbContext).Assembly.FullName))
                .UsePostgreSqlTriggers()
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
            // modelBuilder.Entity<OrderEntryViewModel>
            // another way: https://stackoverflow.com/questions/58693964
            modelBuilder.Entity<OrderViewModel>(e =>
            {
                e.HasNoKey(); // e.HasIndex(c => c.seller_id, "seller_index" )
                e.ToView(nameof(OrderViewModel));
            });

            // trigger event if no viewsupport. to update the table OrderViewModel
            // https://www.postgresql.org/docs/current/sql-refreshmaterializedview.html
            modelBuilder.Entity<OrderEntryViewModel>().AfterInsert(t => t.Action(a => a.ExecuteRawSql($"REFRESH MATERIALIZED VIEW CONCURRENTLY {nameof(OrderViewModel)};")));

        }
    }
}
