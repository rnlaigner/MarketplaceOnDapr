using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using PaymentMS.Models;

namespace PaymentMS.Infra
{
	public class PaymentDbContext : DbContext
    {
		
        public DbSet<OrderPaymentModel> StockItems => Set<OrderPaymentModel>();

        public PaymentDbContext()
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            options.UseNpgsql(@"Host=localhost;Port=5432;Database=payment;Username=postgres;Password=password")
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

            modelBuilder.Entity<OrderPaymentModel>().ToTable(t => t.HasCheckConstraint(
                "CK_OrderPayment_PaymentValue", "payment_value >= 0"
                ));

        }

    }
}

