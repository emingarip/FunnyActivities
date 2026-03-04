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
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
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

            // Don't automatically challenge anonymous requests
            options.Challenge = "Bearer";
            options.Authority = null;

            // Keep custom behavior for anonymous requests without emitting
            // token/claim details to console logs in production.
            options.Events = new JwtBearerEvents
            {
                OnAuthenticationFailed = _ => Task.CompletedTask,
                OnTokenValidated = _ => Task.CompletedTask,
                OnChallenge = _ => Task.CompletedTask,
                OnMessageReceived = context =>
                {
                    // Don't challenge anonymous requests
                    if (string.IsNullOrEmpty(context.Request.Headers.Authorization))
                    {
                        context.NoResult();
                    }
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
