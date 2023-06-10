using System;
using System.Collections.Generic;
using CustomerMS.Models;
using Google.Api;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

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

        public CustomerDbContext()
		{
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

    }
}

