using AspNetCoreRateLimit;
using FluentValidation;
using FluentValidation.AspNetCore;
using FunnyActivities.CrossCuttingConcerns.Authentication;
using FunnyActivities.Domain.Interfaces;
using FunnyActivities.CrossCuttingConcerns.Authorization;
using FunnyActivities.CrossCuttingConcerns.Caching;
using FunnyActivities.CrossCuttingConcerns.ErrorHandling;
using FunnyActivities.CrossCuttingConcerns.Logging;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using NetEscapades.AspNetCore.SecurityHeaders;
using Serilog;
using System.Security.Claims;
using System.Text;

namespace FunnyActivities.CrossCuttingConcerns;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtSettings = configuration.GetSection("JwtSettings").Get<JwtSettings>();
        services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"));

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings?.Issuer,
                ValidAudience = jwtSettings?.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings?.SecretKey ?? "")),
                // Ensure role claims are properly mapped
                RoleClaimType = ClaimTypes.Role,
                NameClaimType = ClaimTypes.NameIdentifier
            };

            // Add event handlers for debugging
            options.Events = new JwtBearerEvents
            {
                OnAuthenticationFailed = context =>
                {
                    Console.WriteLine($"[JWT] Authentication failed: {context.Exception.Message}");
                    return Task.CompletedTask;
                },
                OnTokenValidated = context =>
                {
                    var claims = context.Principal?.Claims.Select(c => $"{c.Type}: {c.Value}").ToList();
                    Console.WriteLine($"[JWT] Token validated successfully. Claims: {string.Join(", ", claims ?? new List<string>())}");

                    // Check for role claims specifically
                    var roleClaims = context.Principal?.Claims.Where(c => c.Type.Contains("role") || c.Type == ClaimTypes.Role).ToList();
                    if (roleClaims?.Any() == true)
                    {
                        Console.WriteLine($"[JWT] Role claims: {string.Join(", ", roleClaims.Select(c => $"{c.Type}={c.Value}"))}");
                    }
                    else
                    {
                        Console.WriteLine($"[JWT] No role claims found in validated token");
                    }

                    return Task.CompletedTask;
                },
                OnChallenge = context =>
                {
                    Console.WriteLine($"[JWT] Challenge triggered: {context.AuthenticateFailure?.Message ?? "No failure details"}");
                    return Task.CompletedTask;
                }
            };
        });

        services.AddScoped<IJwtTokenService, JwtTokenService>();

        return services;
    }

    public static IServiceCollection AddCustomAuthorization(this IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            options.AddPolicy("AdminOnly", policy => policy.Requirements.Add(new AdminRequirement("Admin")));
        });

        services.AddSingleton<IAuthorizationHandler, AdminRequirementHandler>();

        return services;
    }


    public static IServiceCollection AddSerilogLogging(this IServiceCollection services, IConfiguration configuration)
    {
        SerilogConfiguration.ConfigureSerilog(configuration);
        services.AddSerilog();

        return services;
    }

    public static IServiceCollection AddRateLimiting(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMemoryCache();
        services.Configure<IpRateLimitOptions>(configuration.GetSection("IpRateLimiting"));
        services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
        services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
        services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
        services.AddSingleton<IProcessingStrategy, AsyncKeyLockProcessingStrategy>();

        return services;
    }

    public static IServiceCollection AddSecurityHeaders(this IServiceCollection services)
    {
        // TODO: Configure security headers
        // services.AddSecurityHeaderPolicies()
        //     .SetPolicySelector((ctx) => SecurityHeadersDefinitions.GetHeaderPolicyCollection());

        return services;
    }

    public static IServiceCollection AddRedisCaching(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = configuration.GetConnectionString("Redis");
        });

        services.AddScoped<ICacheService, RedisCacheService>();

        return services;
    }
}

public static class ApplicationBuilderExtensions
{
    public static IApplicationBuilder UseCustomExceptionHandling(this IApplicationBuilder app)
    {
        app.UseMiddleware<ExceptionMiddleware>();
        return app;
    }
}