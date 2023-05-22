using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Common.Entities;
using OrderMS.Common.Models;
using Microsoft.Extensions.Logging;

namespace OrderMS.Infra
{
    /**
     * https://www.npgsql.org/efcore/
     * 
     */
    public class OrderDbContext : DbContext
    {
        public DbSet<OrderModel> Orders => Set<OrderModel>();
        public DbSet<OrderItemModel> OrderItems => Set<OrderItemModel>();
        public DbSet<OrderHistoryModel> OrderHistory => Set<OrderHistoryModel>();
        public DbSet<CustomerOrderModel> CustomerOrders => Set<CustomerOrderModel>();

        public OrderDbContext()
        {

        }
        
        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            // connection string can be taken from appsettings:
            // https://jasonwatmore.com/post/2022/06/23/net-6-connect-to-postgresql-database-with-entity-framework-core
            options.UseNpgsql(@"Host=localhost;Port=5432;Database=order;Username=postgres;Password=password")
                .UseLoggerFactory(
                    LoggerFactory.Create(
                        b => b
                            .AddConsole()
                            .AddFilter(level => level >= LogLevel.Information)))
                .EnableSensitiveDataLogging()
                .EnableDetailedErrors();

            options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
        }

        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
            configurationBuilder.Properties<decimal>()
                .HaveColumnType("decimal")
                .HavePrecision(4, 2);
        }

        /**
         * https://www.c-sharpcorner.com/article/exploring-postgresql-sequences-with-entity-framework-core/
         */
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasSequence<long>("OrderNumbers").IncrementsBy(1).StartsAt(1);

            modelBuilder.Entity<OrderModel>()
                .Property(o => o.id)
                .UseIdentityAlwaysColumn() // use ALWAYS db generated identity
                .HasDefaultValueSql("nextval('\"OrderNumbers\"')");

            modelBuilder.HasSequence<long>("OrderHistoryNumbers").IncrementsBy(1).StartsAt(1);

            modelBuilder.Entity<OrderHistoryModel>()
             .Property(o => o.id)
             .UseIdentityAlwaysColumn()
             .HasDefaultValueSql("nextval('\"OrderHistoryNumbers\"')");

        }
        
    }

}

