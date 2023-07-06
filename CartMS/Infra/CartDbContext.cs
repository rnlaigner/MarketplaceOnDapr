using Microsoft.EntityFrameworkCore;
using CartMS.Models;

namespace CartMS.Infra
{
	public class CartDbContext : DbContext
    {
        public DbSet<CartModel> Carts => Set<CartModel>();
        public DbSet<CartItemModel> CartItems => Set<CartItemModel>();
        public DbSet<ProductModel> Products => Set<ProductModel>();
        public DbSet<CartHistoryModel> CartHistory => Set<CartHistoryModel>();

        private readonly IConfiguration configuration;

        public CartDbContext(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            options.UseNpgsql(configuration.GetConnectionString("Database"))
                .EnableSensitiveDataLogging()
                .EnableDetailedErrors();

            options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CartModel>()
                 .Property(e => e.status)
                 .HasConversion<string>();

            modelBuilder.HasSequence<long>("CartHistorySeq").IncrementsBy(1).StartsAt(1);

            modelBuilder.Entity<CartHistoryModel>()
                .Property(o => o.id)
                .UseIdentityAlwaysColumn()
                .HasDefaultValueSql("nextval('\"CartHistorySeq\"')");

            modelBuilder.Entity<CartHistoryModel>()
                 .Property(e => e.status)
                 .HasConversion<string>();

        }

    }
}

