using Microsoft.EntityFrameworkCore;
using CartMS.Models;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace CartMS.Infra
{
	public class CartDbContext : DbContext
    {
        public DbSet<CartModel> Carts => Set<CartModel>();
        public DbSet<CartItemModel> CartItems => Set<CartItemModel>();
        public DbSet<ProductModel> Products => Set<ProductModel>();

        private readonly IConfiguration configuration;

        public CartDbContext(IConfiguration configuration)
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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CartModel>()
                 .Property(e => e.status)
                 .HasConversion<string>();

            // modelBuilder.Entity<CartItemModel>().Property(p => p.vouchers).HasConversion<TagsValueConverter>();
        }

        //public class TagsValueConverter : ValueConverter<IList<decimal>, string>
        //{
        //    public TagsValueConverter() : base(
        //        value => value.Select(v=>v.ToString()).,
        //        dbValue => dbValue.ToList())
        //    {
        //    }
        //}

    }
}

