using System.Transactions;
using Microsoft.EntityFrameworkCore;
using SellerMS.Models;
using Laraue.EfCoreTriggers.PostgreSql.Extensions;
using Google.Api;
using Laraue.EfCoreTriggers.Common.Extensions;
using Common.Utils;

namespace SellerMS.Infra
{
    public class SellerDbContext : DbContext
    {

        public DbSet<OrderView> OrderView => Set<OrderView>();
        public DbSet<OrderSeller> OrderEntries => Set<OrderSeller>();
        
        public DbSet<ShipmentView> ShipmentView => Set<ShipmentView>();
        public DbSet<ShipmentEntry> ShipmentEntries => Set<ShipmentEntry>();

        public DbSet<ProductEntry> ProductEntries => Set<ProductEntry>();

        public SellerDbContext()
        {
        }

        // cant use method, not const...
        // public const string order_sellers_table_name = Utils.FromCamelCaseToUnderscoreLowerCase(nameof(Models.OrderSeller));
        public const string OrderViewSql = $"CREATE MATERIALIZED VIEW IF NOT EXISTS {nameof(Models.OrderView)} AS SELECT seller_id, COUNT(count_items) as count_orders, SUM(total_items) as total_overall, SUM(total_amount) as revenue, AVG(total_items) as avg_order_value, AVG(total_amount) as avg_order_revenue FROM order_sellers GROUP BY seller_id";
        public const string OrderViewSqlIndex = $"CREATE UNIQUE INDEX IF NOT EXISTS seller_order_index ON {nameof(Models.OrderView)} (seller_id)";

        public const string ShipmentViewSql = $"CREATE MATERIALIZED VIEW IF NOT EXISTS {nameof(Models.ShipmentView)} AS SELECT seller_id, COUNT(*) as count_shipments, AVG(delivery_date - shipment_date) AS avg_time_to_complete, AVG(freight_value) as avg_freight_value, SUM(freight_value) as total_freight_amount FROM shipment_entries GROUP BY seller_id";
        public const string ShipmentViewSqlIndex = $"CREATE UNIQUE INDEX IF NOT EXISTS seller_shipment_index ON {nameof(Models.ShipmentView)} (seller_id)";

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            // https://github.com/win7user10/Laraue.EfCoreTriggers#laraueefcoretriggerspostgresql
            options = options
                .UseNpgsql(@"Host=localhost;Port=5432;Database=seller;Username=postgres;Password=password")
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
            // based on: https://khalidabuhakmeh.com/how-to-add-a-view-to-an-entity-framework-core-dbcontext
            modelBuilder.Entity<OrderView>(e =>
            {
                e.HasNoKey();
                e.ToView(nameof(OrderView));
            });

            modelBuilder.Entity<ShipmentView>(e =>
            {
                e.HasNoKey();
                e.ToView(nameof(ShipmentView));
            });

            // trigger event if no viewsupport. to update the table OrderViewModel
            // https://www.postgresql.org/docs/current/sql-refreshmaterializedview.html
            modelBuilder.Entity<OrderSeller>().AfterInsert(t => t.Action(a => a.ExecuteRawSql($"REFRESH MATERIALIZED VIEW CONCURRENTLY {nameof(OrderView)};")));
            modelBuilder.Entity<ShipmentEntry>().AfterInsert(t => t.Action(a => a.ExecuteRawSql($"REFRESH MATERIALIZED VIEW CONCURRENTLY {nameof(ShipmentView)};")));

        }
    }
}
