using System;
using Common.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using PaymentMS.Models;

namespace PaymentMS.Infra
{
	public class PaymentDbContext : DbContext
    {

        public DbSet<OrderPaymentModel> OrderPayments => Set<OrderPaymentModel>();
        public DbSet<OrderPaymentCardModel> OrderPaymentCards => Set<OrderPaymentCardModel>();

        private readonly IConfiguration configuration;

        public PaymentDbContext(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            options.UseNpgsql(configuration.GetConnectionString("Database"))
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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            modelBuilder.Entity<OrderPaymentModel>().ToTable(t => t.HasCheckConstraint(
                "CK_OrderPayment_PaymentValue", "payment_value >= 0"
                ));

            modelBuilder.Entity<OrderPaymentModel>()
                       .Property(e => e.payment_type)
                       .HasConversion<string>();

            modelBuilder.Entity<OrderPaymentModel>()
                .HasOne(e => e.orderPaymentCard)
                .WithOne(c => c.orderPayment)
                .HasForeignKey<OrderPaymentCardModel>(e => new { e.order_id, e.payment_sequential })
                .IsRequired(false);
        }

    }
}

