using System.Linq;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using FunnyActivities.Domain.Entities;
using FunnyActivities.Infrastructure;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FunnyActivities.Api.IntegrationTests
{
    public class CustomWebApplicationFactory : WebApplicationFactory<Program>
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
                    .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("TestAuth", _ => { });

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

    // Dummy auth handler for integration tests
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
