using System.Transactions;
using Microsoft.EntityFrameworkCore;
using SellerMS.Models;
using Laraue.EfCoreTriggers.PostgreSql.Extensions;
using Google.Api;
using Laraue.EfCoreTriggers.Common.Extensions;
using Common.Utils;
using Common.Entities;

namespace SellerMS.Infra
{
    public class SellerDbContext : DbContext
    {

        public DbSet<SellerModel> Sellers => Set<SellerModel>();

        public DbSet<OrderEntry> OrderEntries => Set<OrderEntry>();
        public DbSet<OrderEntryDetails> OrderEntryDetails => Set<OrderEntryDetails>();

        // public DbSet<OrderHistoricalView> OrderHistoricalView => Set<OrderHistoricalView>();

        public DbSet<OrderSellerView> OrderSellerView => Set<OrderSellerView>();
        
        // public DbSet<ShipmentHistoricalView> ShipmentHistoricalView => Set<ShipmentHistoricalView>();
        
        // public DbSet<ProductEntry> ProductEntries => Set<ProductEntry>();

        public SellerDbContext()
        {
        }

        // cant use method, not const... maybe the string being static works...
        // public const string order_sellers_table_name = Utils.FromCamelCaseToUnderscoreLowerCase(nameof(Models.OrderSeller));

        private static string delivered_status = OrderStatus.DELIVERED.ToString();

        public static readonly string OrderHistoricalViewSql = $"CREATE MATERIALIZED VIEW IF NOT EXISTS {nameof(Models.OrderHistoricalView)} " +
                                                                $"AS SELECT os.seller_id, COUNT(os.count_items) as count_orders, SUM(os.total_items) as total_overall, SUM(os.total_amount) as revenue, " +
                                                                $"AVG(os.total_items) as avg_order_value, AVG(os.total_amount) as avg_order_revenue FROM order_entries AS os " +
                                                                $"WHERE os.order_status = \'{delivered_status}\' GROUP BY os.seller_id";

        public const string OrderHistoricalViewSqlIndex = $"CREATE UNIQUE INDEX IF NOT EXISTS seller_order_index ON {nameof(Models.OrderHistoricalView)} (seller_id)";

        public static readonly string ShipmentHistoricalViewSql = $"CREATE MATERIALIZED VIEW IF NOT EXISTS {nameof(Models.ShipmentHistoricalView)} " +
                                                                    $"AS SELECT seller_id, COUNT(*) as count_shipments, AVG(delivery_date - shipment_date) AS avg_time_to_complete, " +
                                                                    $"AVG(freight_value) as avg_freight_value, SUM(freight_value) as total_freight_amount FROM order_entries " +
                                                                    $"WHERE order_status = \'{delivered_status}\' GROUP BY seller_id";

        public const string ShipmentHistoricalViewSqlIndex = $"CREATE UNIQUE INDEX IF NOT EXISTS seller_shipment_index ON {nameof(Models.ShipmentHistoricalView)} (seller_id)";

        // the amount being transacted at the moment
        public static readonly string OrderSellerViewSql = $"CREATE MATERIALIZED VIEW IF NOT EXISTS {nameof(Models.OrderSellerView)} " +
                                                            $"AS SELECT seller_id, COUNT(DISTINCT order_id) as count_orders, COUNT(product_id) as count_items, SUM(total_amount) as total_amount, SUM(freight_value) as total_freight, " +
                                                            $"SUM(total_items - total_amount) as total_incentive, SUM(total_invoice) as total_invoice, SUM(total_items) as total_items " +
                                                            $"FROM order_entries " +
                                                            $"WHERE order_status = \'{OrderStatus.INVOICED.ToString()}\' OR order_status = \'{OrderStatus.PAYMENT_PROCESSED.ToString()}\' " +
                                                            $"OR order_status = \'{OrderStatus.READY_FOR_SHIPMENT.ToString()}\' OR order_status = \'{OrderStatus.IN_TRANSIT.ToString()}\' GROUP BY seller_id";

        public const string OrderSellerViewSqlIndex = $"CREATE UNIQUE INDEX IF NOT EXISTS order_seller_index ON {nameof(Models.OrderSellerView)} (seller_id)";


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
            // https://stackoverflow.com/questions/60285154/how-to-set-partial-index-with-npgsql
            modelBuilder.Entity<OrderEntry>(e =>
            {
                e.HasIndex(oe => oe.order_status, "order_entry_open_idx").HasFilter($"order_status = \'{OrderStatus.INVOICED.ToString()}\' OR order_status = \'{OrderStatus.PAYMENT_PROCESSED.ToString()}\' OR order_status = \'{OrderStatus.READY_FOR_SHIPMENT.ToString()}\' OR order_status = \'{OrderStatus.IN_TRANSIT.ToString()}\'");
                // e.HasIndex(oe => oe.order_status, "order_entry_delivered_idx").HasFilter($"\'status\' = \'{OrderStatus.DELIVERED.ToString()}\'");

                e.Property(e => e.order_status)
                       .HasConversion<string>();
                e.Property(e => e.delivery_status)
                       .HasConversion<string>();
            });

            // https://stackoverflow.com/questions/7437952/map-string-column-in-entity-framework-to-enum
            modelBuilder.Entity<OrderEntryDetails>().Property(e => e.status).HasConversion<string>();

            // modelBuilder.Entity<OrderEntryViewModel>
            // another way: https://stackoverflow.com/questions/58693964
            // based on: https://khalidabuhakmeh.com/how-to-add-a-view-to-an-entity-framework-core-dbcontext
            modelBuilder.Entity<OrderSellerView>(e =>
            {
                e.HasNoKey();
                e.ToView(nameof(OrderSellerView));
            });

            /*
            modelBuilder.Entity<OrderHistoricalView>(e =>
            {
                e.HasNoKey();
                e.ToView(nameof(OrderHistoricalView));
            });

            modelBuilder.Entity<ShipmentHistoricalView>(e =>
            {
                e.HasNoKey();
                e.ToView(nameof(ShipmentHistoricalView));
            });
            */

            // TODO discuss since this will impact significantly performance. perhaps sellers access materialized view few times, but that does not change the cost of rebuilding the view
            // can we specify a partition by seller_id to improve scans of order_entries?
            // trigger event if no viewsupport. to update the table OrderViewModel
            // https://www.postgresql.org/docs/current/sql-refreshmaterializedview.html
            // https://stackoverflow.com/questions/29437650/how-can-i-ensure-that-a-materialized-view-is-always-up-to-date

            // it seems the library does not accept after insert or update or delete. in this case it is necessary to edit the migration directly
            //modelBuilder.Entity<OrderEntry>().AfterInsert(t => t.Action(a => a.ExecuteRawSql($"REFRESH MATERIALIZED VIEW CONCURRENTLY {nameof(OrderHistoricalView)};")));
            //modelBuilder.Entity<OrderEntry>().AfterInsert(t => t.Action(a => a.ExecuteRawSql($"REFRESH MATERIALIZED VIEW CONCURRENTLY {nameof(ShipmentHistoricalView)};")));
            modelBuilder.Entity<OrderEntryDetails>().AfterInsert(t => t.Action(a => a.ExecuteRawSql($"REFRESH MATERIALIZED VIEW CONCURRENTLY {nameof(Models.OrderSellerView)};")));
            modelBuilder.Entity<OrderEntryDetails>().AfterUpdate(t => t.Action(a => a.ExecuteRawSql($"REFRESH MATERIALIZED VIEW CONCURRENTLY {nameof(Models.OrderSellerView)};")));

        }
    }
}
