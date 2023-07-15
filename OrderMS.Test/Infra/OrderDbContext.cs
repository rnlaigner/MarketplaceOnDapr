using Microsoft.EntityFrameworkCore;
using OrderMS.Common.Models;
using Microsoft.Extensions.Logging;

namespace OrderMS.Test.Infra
{
    public class OrderDbContext : DbContext
    {
        public DbSet<OrderModel> Orders => Set<OrderModel>();
        public DbSet<OrderItemModel> OrderItems => Set<OrderItemModel>();
        public DbSet<OrderHistoryModel> OrderHistory => Set<OrderHistoryModel>();

        public string DbPath { get; }

        public OrderDbContext()
        {
            var folder = Environment.SpecialFolder.LocalApplicationData;
            var path = Environment.GetFolderPath(folder);
            this.DbPath = Path.Join(path, "order.db");
        }

        // The following configures EF to create a Sqlite database file in the
        // special "local" folder for your platform.
        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            Console.WriteLine("[OnConfiguring] DbPath: {0}", this.DbPath);

            // https://stackoverflow.com/questions/63115500/efcore-does-not-create-all-the-columns-in-the-database
            options.UseSqlite($"Data Source={this.DbPath}").UseLoggerFactory(
                    LoggerFactory.Create(
                        b => b
                            .AddConsole()
                            .AddFilter(level => level >= LogLevel.Information)))
                .EnableSensitiveDataLogging()
                .EnableDetailedErrors();
            options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
        }

        /*
         * 
         * https://stackoverflow.com/questions/43277154
         */
        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
            configurationBuilder.Properties<decimal>()
                .HaveColumnType("NUMERIC")
                .HavePrecision(4, 2);

            configurationBuilder.Properties<DateTime>()
                .HaveColumnType("Date");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // modelBuilder.
            //modelBuilder.Entity<OrderModel>().HasCheckConstraint()

            //modelBuilder.ApplyConfiguration(new OrderEntityTypeConfiguration());
            // modelBuilder.ApplyConfiguration(new OrderItemEntityTypeConfiguration());
        }
        
    }

}

