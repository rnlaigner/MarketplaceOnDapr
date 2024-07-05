using Microsoft.EntityFrameworkCore;
using SellerMS.Models;
using Laraue.EfCoreTriggers.PostgreSql.Extensions;
using Common.Entities;

namespace SellerMS.Infra
{
    public class SellerDbContext : DbContext
    {
        public DbSet<SellerModel> Sellers => Set<SellerModel>();

        public DbSet<OrderEntry> OrderEntries => Set<OrderEntry>();

        public DbSet<OrderSellerView> OrderSellerView => Set<OrderSellerView>();

        private readonly IConfiguration configuration;

        public SellerDbContext(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public static readonly string SELLER_VIEW_NAME = nameof(Models.OrderSellerView).ToLower();

        public static readonly string ORDER_SELLER_VIEW_UPDATE_SQL = $"REFRESH MATERIALIZED VIEW CONCURRENTLY seller.{SELLER_VIEW_NAME}";

        // the amount being transacted at the moment
        public static readonly string ORDER_SELLER_VIEW_SQL = $"CREATE MATERIALIZED VIEW IF NOT EXISTS seller.{SELLER_VIEW_NAME} " +
                                                            $"AS SELECT seller_id, COUNT(DISTINCT natural_key) as count_orders, COUNT(product_id) as count_items, SUM(total_amount) as total_amount, SUM(freight_value) as total_freight, " +
                                                            $"SUM(total_items - total_amount) as total_incentive, SUM(total_invoice) as total_invoice, SUM(total_items) as total_items " +
                                                            $"FROM seller.order_entries " +
                                                            $"WHERE order_status = \'{OrderStatus.INVOICED}\' OR order_status = \'{OrderStatus.PAYMENT_PROCESSED}\' " +
                                                            $"OR order_status = \'{OrderStatus.READY_FOR_SHIPMENT}\' OR order_status = \'{OrderStatus.IN_TRANSIT}\' GROUP BY seller_id";

        public static readonly string ORDER_SELLER_VIEW_SQL_INDEX = $"CREATE UNIQUE INDEX IF NOT EXISTS order_seller_index ON seller.{SELLER_VIEW_NAME} (seller_id)";

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            // https://github.com/win7user10/Laraue.EfCoreTriggers#laraueefcoretriggerspostgresql
            options
                .UseNpgsql(this.configuration.GetConnectionString("Database"))
                .UsePostgreSqlTriggers()
                .EnableSensitiveDataLogging()
                .EnableDetailedErrors()
                .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
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

            // another way: https://stackoverflow.com/questions/58693964
            // based on: https://khalidabuhakmeh.com/how-to-add-a-view-to-an-entity-framework-core-dbcontext
            modelBuilder.Entity<OrderSellerView>(e =>
            {
                e.HasNoKey();
                e.ToView(SELLER_VIEW_NAME, schema: "seller");
            });
        }
    }
}
