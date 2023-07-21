using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Common.Entities;
using OrderMS.Common.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace OrderMS.Infra
{

    public class OrderDbContext : DbContext
    {
        public DbSet<OrderModel> Orders => Set<OrderModel>();
        public DbSet<OrderItemModel> OrderItems => Set<OrderItemModel>();
        public DbSet<OrderHistoryModel> OrderHistory => Set<OrderHistoryModel>();
        public DbSet<CustomerOrderModel> CustomerOrders => Set<CustomerOrderModel>();

        private readonly IConfiguration configuration;

        public OrderDbContext(IConfiguration configuration)
        {
            this.configuration = configuration;
        }
        
        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            // connection string can be taken from appsettings:
            // https://jasonwatmore.com/post/2022/06/23/net-6-connect-to-postgresql-database-with-entity-framework-core
            options.UseNpgsql(configuration.GetConnectionString("Database"))
                // .AddInterceptors(new TransactionInterceptor())
                .EnableSensitiveDataLogging()
                .EnableDetailedErrors();

            options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
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

            modelBuilder.Entity<OrderModel>().Property(e => e.status).HasConversion<string>();
            modelBuilder.Entity<OrderHistoryModel>().Property(e => e.status).HasConversion<string>();
        }
        
    }

}

