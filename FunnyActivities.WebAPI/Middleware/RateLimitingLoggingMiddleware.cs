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

            // Survey-specific rate limiting logic
            var surveyRateLimitInfo = GetSurveyRateLimitInfo(endpoint, method, context.User.Identity?.IsAuthenticated == true);

            // Check for rate limiting headers (assuming AspNetCoreRateLimit is used)
            if (context.Response.StatusCode == 429)
            {
                var retryAfter = context.Response.Headers["Retry-After"].FirstOrDefault();
                var limit = context.Response.Headers["X-Rate-Limit-Limit"].FirstOrDefault();

                // Enhanced logging for survey endpoints
                if (IsSurveyEndpoint(endpoint))
                {
                    _logger.LogWarning("Survey rate limit exceeded for request",
                        new
                        {
                            IPAddress = MaskIP(ipAddress),
                            Endpoint = endpoint,
                            Method = method,
                            RetryAfter = retryAfter,
                            Limit = limit,
                            CorrelationId = correlationId,
                            IsAuthenticated = context.User.Identity?.IsAuthenticated,
                            UserRole = context.User.Identity?.IsAuthenticated == true ? context.User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value : "Anonymous",
                            RecommendedLimit = surveyRateLimitInfo.RecommendedLimit,
                            EndpointType = surveyRateLimitInfo.EndpointType
                        });

                    _securityLogger.LogRateLimitExceeded(ipAddress, endpoint, int.TryParse(limit, out var l) ? l : 0, correlationId);
                }
                else
                {
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
            }

            // Log survey-specific rate limiting information
            if (IsSurveyEndpoint(endpoint))
            {
                _logger.LogDebug("Survey endpoint accessed with rate limit info",
                    new
                    {
                        IPAddress = MaskIP(ipAddress),
                        Endpoint = endpoint,
                        Method = method,
                        CorrelationId = correlationId,
                        IsAuthenticated = context.User.Identity?.IsAuthenticated,
                        EndpointType = surveyRateLimitInfo.EndpointType,
                        RecommendedLimit = surveyRateLimitInfo.RecommendedLimit,
                        IsHighTraffic = surveyRateLimitInfo.IsHighTraffic
                    });
            }

            await _next(context);
        }

        private bool IsSurveyEndpoint(string endpoint)
        {
            return endpoint.Contains("/api/surveys") || endpoint.Contains("/api/public-surveys");
        }

        private (string EndpointType, int RecommendedLimit, bool IsHighTraffic) GetSurveyRateLimitInfo(string endpoint, string method, bool isAuthenticated)
        {
            // Public survey endpoints - higher rate limits for anonymous users
            if (endpoint.Contains("/api/public-surveys"))
            {
                if (method == "GET")
                {
                    return ("PublicSurveyRead", isAuthenticated ? 1000 : 2000, true);
                }
                else if (method == "POST")
                {
                    return ("PublicSurveyVote", isAuthenticated ? 100 : 200, true);
                }
            }

            // Admin survey endpoints - lower rate limits for security
            if (endpoint.Contains("/api/surveys") && (endpoint.Contains("/admin") || endpoint.Contains("/statistics")))
            {
                return ("AdminSurvey", 100, false);
            }

            // Regular survey management endpoints
            if (endpoint.Contains("/api/surveys"))
            {
                if (method == "GET")
                {
                    return ("SurveyManagementRead", 500, false);
                }
                else
                {
                    return ("SurveyManagementWrite", 200, false);
                }
            }

            return ("Unknown", 100, false);
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