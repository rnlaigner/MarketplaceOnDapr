using CustomerMS.Models;
using Microsoft.EntityFrameworkCore;

namespace CustomerMS.Infra
{
	public class CustomerDbContext : DbContext
    {

        public DbSet<CustomerModel> Customers => Set<CustomerModel>();

        private readonly IConfiguration configuration;

        public CustomerDbContext(IConfiguration configuration)
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

    }
}

