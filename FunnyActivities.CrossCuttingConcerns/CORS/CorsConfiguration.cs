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
        });

        return services;
    }
}