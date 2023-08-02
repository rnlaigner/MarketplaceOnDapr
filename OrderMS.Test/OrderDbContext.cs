using Microsoft.EntityFrameworkCore;
using OrderMS.Common.Models;
using Microsoft.Extensions.Configuration;

namespace OrderMS.Infra
{

    public class OrderDbContext : DbContext
    {
        public DbSet<OrderModel> Orders => Set<OrderModel>();
        public DbSet<OrderItemModel> OrderItems => Set<OrderItemModel>();
        public DbSet<OrderHistoryModel> OrderHistory => Set<OrderHistoryModel>();
        public DbSet<CustomerOrderModel> CustomerOrders => Set<CustomerOrderModel>();

        private readonly string ConnectionString;

        public OrderDbContext(IConfiguration configuration)
        {
            this.ConnectionString = configuration.GetConnectionString("Database") ?? throw new Exception("Unknown connection string");
        }

        public OrderDbContext(string ConnectionString)
        {
            this.ConnectionString = ConnectionString;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            // connection string can be taken from appsettings:
            // https://jasonwatmore.com/post/2022/06/23/net-6-connect-to-postgresql-database-with-entity-framework-core
            options.UseNpgsql(this.ConnectionString)
                .EnableSensitiveDataLogging()
                .EnableDetailedErrors();
        }

        /**
         * https://www.c-sharpcorner.com/article/exploring-postgresql-sequences-with-entity-framework-core/
         */
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("order");

            modelBuilder.HasSequence<int>("OrderNumbers").IncrementsBy(1).StartsAt(1);

            modelBuilder.Entity<OrderModel>()
                .Property(o => o.id)
                .UseIdentityAlwaysColumn() // use ALWAYS db generated identity
                .HasDefaultValueSql("nextval('\"OrderNumbers\"')");

            modelBuilder.HasSequence<int>("OrderHistoryNumbers").IncrementsBy(1).StartsAt(1);

            modelBuilder.Entity<OrderHistoryModel>()
             .Property(o => o.id)
             .UseIdentityAlwaysColumn()
             .HasDefaultValueSql("nextval('\"OrderHistoryNumbers\"')");

            modelBuilder.Entity<OrderModel>().Property(e => e.status).HasConversion<string>();
            modelBuilder.Entity<OrderHistoryModel>().Property(e => e.status).HasConversion<string>();
        }
        
    }

}

