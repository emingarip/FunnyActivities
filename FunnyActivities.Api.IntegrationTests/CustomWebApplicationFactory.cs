using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using FunnyActivities.WebAPI;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using FunnyActivities.Infrastructure;
using FunnyActivities.Domain.Entities;

namespace FunnyActivities.Api.IntegrationTests
{
    public class CustomWebApplicationFactory : WebApplicationFactory<FunnyActivities.WebAPI.TestEntryPoint>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                // Remove all existing DbContext registrations
                var dbContextDescriptors = services.Where(
                    d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>) ||
                         d.ServiceType == typeof(ApplicationDbContext)).ToList();

                foreach (var descriptor in dbContextDescriptors)
                {
                    services.Remove(descriptor);
                }

                // Remove database provider services to avoid conflicts
                var providerDescriptors = services.Where(
                    d => d.ServiceType.FullName?.Contains("Npgsql") == true ||
                         d.ServiceType.FullName?.Contains("EntityFrameworkCore") == true).ToList();

                foreach (var descriptor in providerDescriptors)
                {
                    services.Remove(descriptor);
                }

                // Configure ApplicationDbContext to use InMemory database for testing
                services.AddDbContext<ApplicationDbContext>(options =>
                {
                    options.UseInMemoryDatabase("TestDatabase");
                });

                // Configure test-specific services here if needed
                // For example, mock external services
            });

            builder.ConfigureAppConfiguration((context, config) =>
            {
                // Use test configuration
                config.AddJsonFile("appsettings.Test.json", optional: true);
            });
        }

    }

    // Dummy Startup class to satisfy the generic constraint
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            // This will be overridden by the actual Program.cs configuration
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // This will be overridden by the actual Program.cs configuration
        }
    }
}