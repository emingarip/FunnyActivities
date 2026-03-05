using System.Linq;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using FunnyActivities.Application.Interfaces;
using FunnyActivities.CrossCuttingConcerns.Caching;
using FunnyActivities.Domain.Entities;
using FunnyActivities.Infrastructure;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace FunnyActivities.Api.IntegrationTests
{
    public class CustomWebApplicationFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Development");

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

                // Mock authentication for testing and set as default
                services.AddAuthentication(options =>
                    {
                        options.DefaultAuthenticateScheme = "TestAuth";
                        options.DefaultChallengeScheme = "TestAuth";
                    })
                    .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("TestAuth", _ => { });

                // Replace external dependencies with in-memory/no-op implementations
                services.RemoveAll<ILlmSettingsInitializer>();
                services.AddSingleton<ILlmSettingsInitializer, NoOpLlmSettingsInitializer>();

                services.RemoveAll<IConnectionMultiplexer>();
                services.RemoveAll<IDistributedCache>();
                services.RemoveAll<ICacheService>();
                services.AddDistributedMemoryCache();
                services.AddSingleton<ICacheService, InMemoryCacheService>();

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
                new Claim(ClaimTypes.NameIdentifier, "11111111-1111-1111-1111-111111111111"),
                new Claim(ClaimTypes.Email, "test@test.com"),
                new Claim(ClaimTypes.Name, "test-user"),
                new Claim(ClaimTypes.Role, "Admin")
            };

            var identity = new ClaimsIdentity(claims, "TestAuth");
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, "TestAuth");

            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }

    internal class NoOpLlmSettingsInitializer : ILlmSettingsInitializer
    {
        public Task InitializeAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    internal class InMemoryCacheService : ICacheService
    {
        private readonly Dictionary<string, object?> _store = new();
        private readonly object _lock = new();

        public Task<T?> GetAsync<T>(string key)
        {
            lock (_lock)
            {
                return Task.FromResult(_store.TryGetValue(key, out var value) ? (T?)value : default);
            }
        }

        public Task SetAsync<T>(string key, T value, TimeSpan? expiry = null)
        {
            lock (_lock)
            {
                _store[key] = value;
                return Task.CompletedTask;
            }
        }

        public Task RemoveAsync(string key)
        {
            lock (_lock)
            {
                _store.Remove(key);
                return Task.CompletedTask;
            }
        }

        public Task<bool> ExistsAsync(string key)
        {
            lock (_lock)
            {
                return Task.FromResult(_store.ContainsKey(key));
            }
        }
    }
}
