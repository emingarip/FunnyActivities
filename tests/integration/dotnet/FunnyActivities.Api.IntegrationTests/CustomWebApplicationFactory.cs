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
    public class CustomWebApplicationFactory : WebApplicationFactory<FunnyActivities.WebAPI.Program>
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

                // Mock authentication for testing
                services.AddAuthentication("TestAuth")
                    .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("TestAuth", options =>
                    {
                        options.DisplayName = "Test Auth";
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
    public class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public TestAuthHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock)
            : base(options, logger, encoder, clock)
        {
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "test-user-id"),
                new Claim(ClaimTypes.Email, "test@test.com"),
                new Claim(ClaimTypes.Role, "Admin")
            };

            var identity = new ClaimsIdentity(claims, "TestAuth");
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, "TestAuth");

            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }
}