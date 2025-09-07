using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using FunnyActivities.CrossCuttingConcerns.Logging;
using System.Threading.Tasks;

namespace FunnyActivities.WebAPI.Middleware
{
    public class RateLimitingLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RateLimitingLoggingMiddleware> _logger;
        private readonly SecurityEventLogger _securityLogger;

        public RateLimitingLoggingMiddleware(
            RequestDelegate next,
            ILogger<RateLimitingLoggingMiddleware> logger,
            SecurityEventLogger securityLogger)
        {
            _next = next;
            _logger = logger;
            _securityLogger = securityLogger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var endpoint = context.Request.Path.ToString();
            var method = context.Request.Method;
            var correlationId = context.Request.Headers["X-Correlation-ID"].FirstOrDefault() ?? Guid.NewGuid().ToString();

            // Check for rate limiting headers (assuming AspNetCoreRateLimit is used)
            if (context.Response.StatusCode == 429)
            {
                var retryAfter = context.Response.Headers["Retry-After"].FirstOrDefault();
                var limit = context.Response.Headers["X-Rate-Limit-Limit"].FirstOrDefault();

                _logger.LogWarning("Rate limit exceeded for request",
                    new
                    {
                        IPAddress = MaskIP(ipAddress),
                        Endpoint = endpoint,
                        Method = method,
                        RetryAfter = retryAfter,
                        Limit = limit,
                        CorrelationId = correlationId
                    });

                _securityLogger.LogRateLimitExceeded(ipAddress, endpoint, int.TryParse(limit, out var l) ? l : 0, correlationId);
            }

            await _next(context);
        }

        private string MaskIP(string ip)
        {
            if (string.IsNullOrEmpty(ip) || ip == "unknown") return ip;
            var parts = ip.Split('.');
            if (parts.Length >= 1)
                return parts[0] + ".***.***.***";
            return "***.***.***.***";
        }
    }
}