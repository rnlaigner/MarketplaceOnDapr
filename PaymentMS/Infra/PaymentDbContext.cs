using Microsoft.EntityFrameworkCore;
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
                .EnableSensitiveDataLogging()
                .EnableDetailedErrors()
                .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("payment");
            modelBuilder.Entity<OrderPaymentModel>().ToTable(t => t.HasCheckConstraint(
                "CK_OrderPayment_Value", "value >= 0"
                ));

            modelBuilder.Entity<OrderPaymentModel>()
                       .Property(e => e.type)
                       .HasConversion<string>();

            modelBuilder.Entity<OrderPaymentModel>()
                       .Property(e => e.status)
                       .HasConversion<string>();

            modelBuilder.Entity<OrderPaymentModel>()
                .HasOne(e => e.orderPaymentCard)
                .WithOne(c => c.orderPayment)
                .HasForeignKey<OrderPaymentCardModel>(e => new { e.order_id, e.payment_sequential })
                .IsRequired(false);
        }

    }
}

