using FunnyActivities.WebAPI.Middleware;
using Microsoft.AspNetCore.Builder;

namespace FunnyActivities.WebAPI.Extensions
{
    /// <summary>
    /// Extension methods for registering custom middleware components.
    /// </summary>
    public static class MiddlewareExtensions
    {
        /// <summary>
        /// Adds the authentication middleware to the application pipeline.
        /// This middleware validates user authentication and sets up user context.
        /// </summary>
        /// <param name="app">The application builder instance.</param>
        /// <returns>The application builder with authentication middleware added.</returns>
        public static IApplicationBuilder UseAuthenticationMiddleware(this IApplicationBuilder app)
        {
            return app.UseMiddleware<AuthenticationMiddleware>();
        }

        /// <summary>
        /// Adds request/response logging middleware to the application pipeline.
        /// This middleware logs HTTP requests and responses with PII-safe data.
        /// </summary>
        /// <param name="app">The application builder instance.</param>
        /// <returns>The application builder with request/response logging middleware added.</returns>
        public static IApplicationBuilder UseRequestResponseLogging(this IApplicationBuilder app)
        {
            return app.UseMiddleware<RequestResponseLoggingMiddleware>();
        }

        /// <summary>
        /// Adds rate limiting logging middleware to the application pipeline.
        /// This middleware logs rate limiting events and security violations.
        /// </summary>
        /// <param name="app">The application builder instance.</param>
        /// <returns>The application builder with rate limiting logging middleware added.</returns>
        public static IApplicationBuilder UseRateLimitingLogging(this IApplicationBuilder app)
        {
            return app.UseMiddleware<RateLimitingLoggingMiddleware>();
        }

        /// <summary>
        /// Adds caching logging middleware to the application pipeline.
        /// This middleware logs cache hits, misses, and invalidation events.
        /// </summary>
        /// <param name="app">The application builder instance.</param>
        /// <returns>The application builder with caching logging middleware added.</returns>
        public static IApplicationBuilder UseCachingLogging(this IApplicationBuilder app)
        {
            return app.UseMiddleware<CachingLoggingMiddleware>();
        }
    }
}