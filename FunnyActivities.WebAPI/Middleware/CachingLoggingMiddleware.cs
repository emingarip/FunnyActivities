using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Threading.Tasks;

namespace FunnyActivities.WebAPI.Middleware
{
    public class CachingLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<CachingLoggingMiddleware> _logger;

        public CachingLoggingMiddleware(RequestDelegate next, ILogger<CachingLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var stopwatch = Stopwatch.StartNew();
            var cacheStatus = "miss"; // Default assumption
            var correlationId = context.Request.Headers["X-Correlation-ID"].FirstOrDefault() ?? Guid.NewGuid().ToString();

            // Check for cache headers
            if (context.Response.Headers.ContainsKey("X-Cache-Status"))
            {
                cacheStatus = context.Response.Headers["X-Cache-Status"];
            }
            else if (context.Response.Headers.ContainsKey("X-Cache"))
            {
                cacheStatus = context.Response.Headers["X-Cache"];
            }

            await _next(context);

            stopwatch.Stop();

            // Log cache performance
            if (cacheStatus.Equals("hit", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogInformation("Cache hit for request",
                    new
                    {
                        Endpoint = context.Request.Path.ToString(),
                        Method = context.Request.Method,
                        CacheStatus = cacheStatus,
                        DurationMs = stopwatch.ElapsedMilliseconds,
                        CorrelationId = correlationId
                    });
            }
            else if (cacheStatus.Equals("miss", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogInformation("Cache miss for request",
                    new
                    {
                        Endpoint = context.Request.Path.ToString(),
                        Method = context.Request.Method,
                        CacheStatus = cacheStatus,
                        DurationMs = stopwatch.ElapsedMilliseconds,
                        CorrelationId = correlationId
                    });
            }

            // Log cache invalidation if header is present
            if (context.Response.Headers.ContainsKey("X-Cache-Invalidation"))
            {
                var invalidationReason = context.Response.Headers["X-Cache-Invalidation"];
                _logger.LogInformation("Cache invalidation performed",
                    new
                    {
                        Endpoint = context.Request.Path.ToString(),
                        Method = context.Request.Method,
                        InvalidationReason = invalidationReason,
                        DurationMs = stopwatch.ElapsedMilliseconds,
                        CorrelationId = correlationId
                    });
            }
        }
    }
}