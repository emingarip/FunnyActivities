using Microsoft.Extensions.DependencyInjection;

namespace FunnyActivities.CrossCuttingConcerns.CORS;

public static class CorsConfiguration
{
    public static IServiceCollection AddCustomCors(this IServiceCollection services, string[] allowedOrigins)
    {
        services.AddCors(options =>
        {
            options.AddPolicy("AllowSpecificOrigins", builder =>
            {
                builder.WithOrigins(allowedOrigins)
                       .AllowAnyMethod()
                       .AllowAnyHeader()
                       .AllowCredentials();
            });

            // Allow all origins for development (temporary fix)
            options.AddPolicy("AllowAllOrigins", builder =>
            {
                builder.AllowAnyOrigin()
                       .AllowAnyMethod()
                       .AllowAnyHeader();
                // Note: AllowCredentials() cannot be used with AllowAnyOrigin()
                // For development, we allow all origins without credentials
            });

            // Survey-specific CORS policies
            options.AddPolicy("SurveyPublicAccess", builder =>
            {
                builder.WithOrigins(allowedOrigins)
                       .WithMethods("GET", "POST", "OPTIONS")
                       .WithHeaders("Content-Type", "Authorization", "X-Requested-With", "Accept", "Origin")
                       .AllowCredentials()
                       .SetPreflightMaxAge(TimeSpan.FromMinutes(10));
            });

            // More restrictive CORS for admin survey endpoints
            options.AddPolicy("SurveyAdminAccess", builder =>
            {
                builder.WithOrigins(allowedOrigins)
                       .WithMethods("GET", "POST", "PUT", "DELETE", "OPTIONS")
                       .WithHeaders("Content-Type", "Authorization", "X-Requested-With", "Accept", "Origin", "X-Correlation-ID")
                       .AllowCredentials()
                       .SetPreflightMaxAge(TimeSpan.FromMinutes(10));
            });

            // Public survey endpoints with minimal restrictions
            options.AddPolicy("PublicSurveyOnly", builder =>
            {
                builder.WithOrigins(allowedOrigins)
                       .WithMethods("GET", "POST", "OPTIONS")
                       .WithHeaders("Content-Type", "Accept", "Origin")
                       .SetPreflightMaxAge(TimeSpan.FromMinutes(30));
                // Note: No credentials allowed for public endpoints
            });
        });

        return services;
    }
}