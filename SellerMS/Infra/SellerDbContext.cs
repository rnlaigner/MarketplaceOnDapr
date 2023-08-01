using Microsoft.EntityFrameworkCore;
using SellerMS.Models;
using Laraue.EfCoreTriggers.PostgreSql.Extensions;
using Laraue.EfCoreTriggers.Common.Extensions;
using Common.Entities;

namespace SellerMS.Infra
{
    public class SellerDbContext : DbContext
    {

        public DbSet<SellerModel> Sellers => Set<SellerModel>();

        public DbSet<OrderEntry> OrderEntries => Set<OrderEntry>();
        public DbSet<OrderEntryDetails> OrderEntryDetails => Set<OrderEntryDetails>();

        public DbSet<OrderSellerView> OrderSellerView => Set<OrderSellerView>();

        private readonly IConfiguration configuration;

        public SellerDbContext(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        // the amount being transacted at the moment
        public static readonly string OrderSellerViewSql = $"CREATE MATERIALIZED VIEW IF NOT EXISTS {nameof(Models.OrderSellerView)} " +
                                                            $"AS SELECT seller_id, COUNT(DISTINCT order_id) as count_orders, COUNT(product_id) as count_items, SUM(total_amount) as total_amount, SUM(freight_value) as total_freight, " +
                                                            $"SUM(total_items - total_amount) as total_incentive, SUM(total_invoice) as total_invoice, SUM(total_items) as total_items " +
                                                            $"FROM order_entries " +
                                                            $"WHERE order_status = \'{OrderStatus.INVOICED}\' OR order_status = \'{OrderStatus.PAYMENT_PROCESSED}\' " +
                                                            $"OR order_status = \'{OrderStatus.READY_FOR_SHIPMENT}\' OR order_status = \'{OrderStatus.IN_TRANSIT}\' GROUP BY seller_id";

        public const string OrderSellerViewSqlIndex = $"CREATE UNIQUE INDEX IF NOT EXISTS order_seller_index ON {nameof(Models.OrderSellerView)} (seller_id)";

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            // https://github.com/win7user10/Laraue.EfCoreTriggers#laraueefcoretriggerspostgresql
            options = options
                .UseNpgsql(configuration.GetConnectionString("Database"))
                .UsePostgreSqlTriggers()
                .EnableSensitiveDataLogging()
                .EnableDetailedErrors();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("seller");
            // https://stackoverflow.com/questions/60285154/how-to-set-partial-index-with-npgsql
            modelBuilder.Entity<OrderEntry>(e =>
            {
                e.HasIndex(oe => oe.order_status, "order_entry_open_idx").HasFilter($"order_status = \'{OrderStatus.INVOICED}\' OR order_status = \'{OrderStatus.PAYMENT_PROCESSED}\' OR order_status = \'{OrderStatus.READY_FOR_SHIPMENT}\' OR order_status = \'{OrderStatus.IN_TRANSIT}\'");
                e.Property(e => e.order_status)
                       .HasConversion<string>();
                e.Property(e => e.delivery_status)
                       .HasConversion<string>();
            });

            // https://stackoverflow.com/questions/7437952/map-string-column-in-entity-framework-to-enum
            modelBuilder.Entity<OrderEntryDetails>().Property(e => e.status).HasConversion<string>();

            // another way: https://stackoverflow.com/questions/58693964
            // based on: https://khalidabuhakmeh.com/how-to-add-a-view-to-an-entity-framework-core-dbcontext
            modelBuilder.Entity<OrderSellerView>(e =>
            {
                e.HasNoKey();
                e.ToView(nameof(OrderSellerView));
            });

            // library does not support (after insert or update)
            modelBuilder.Entity<OrderEntryDetails>().AfterInsert(t => t.Action(a => a.ExecuteRawSql($"REFRESH MATERIALIZED VIEW CONCURRENTLY {nameof(Models.OrderSellerView)};")));
            modelBuilder.Entity<OrderEntryDetails>().AfterUpdate(t => t.Action(a => a.ExecuteRawSql($"REFRESH MATERIALIZED VIEW CONCURRENTLY {nameof(Models.OrderSellerView)};")));

        }
    }
}
