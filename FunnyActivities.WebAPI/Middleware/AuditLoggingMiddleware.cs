using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using FunnyActivities.Application.Interfaces;
using FunnyActivities.Domain.Entities;

namespace FunnyActivities.WebAPI.Middleware
{
    public class AuditLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IServiceProvider _serviceProvider;

        public AuditLoggingMiddleware(RequestDelegate next, IServiceProvider serviceProvider)
        {
            _next = next;
            _serviceProvider = serviceProvider;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var correlationId = context.Request.Headers["X-Correlation-ID"].FirstOrDefault() ?? Guid.NewGuid().ToString();
            var path = context.Request.Path.Value;
            var method = context.Request.Method;

            // Add correlation ID to response for debugging
            context.Response.Headers["X-Correlation-ID"] = correlationId;

            Serilog.Log.Information("[AUDIT-MW] Starting audit logging for {Method} {Path}, CorrelationId: {CorrelationId}",
                method, path, correlationId);

            try
            {
                // Get IP and UserAgent
                var ipAddress = context.Connection.RemoteIpAddress?.ToString();
                var userAgent = context.Request.Headers["User-Agent"].ToString();

                Serilog.Log.Debug("[AUDIT-MW] IP: {IP}, UserAgent: {UserAgent}, CorrelationId: {CorrelationId}",
                    ipAddress, userAgent, correlationId);

                // Get user ID if authenticated
                Guid? userId = null;
                if (context.User.Identity.IsAuthenticated)
                {
                    var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier);
                    if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var id))
                    {
                        userId = id;
                    }
                }

                Serilog.Log.Debug("[AUDIT-MW] User authenticated: {IsAuthenticated}, UserId: {UserId}, CorrelationId: {CorrelationId}",
                    context.User.Identity.IsAuthenticated, userId, correlationId);

                // Determine action based on path and method
                string action = GetActionFromRequest(path, method);

                Serilog.Log.Debug("[AUDIT-MW] Determined action: {Action}, CorrelationId: {CorrelationId}",
                    action, correlationId);

                // Temporarily skip audit logging for login endpoint to test if middleware is causing the hang
                if (!string.IsNullOrEmpty(action) && action != "UserLogin")
                {
                    Serilog.Log.Information("[AUDIT-MW] Creating audit log for action: {Action}, CorrelationId: {CorrelationId}",
                        action, correlationId);

                    try
                    {
                        using (var scope = _serviceProvider.CreateScope())
                        {
                            Serilog.Log.Debug("[AUDIT-MW] Created service scope, CorrelationId: {CorrelationId}", correlationId);

                            var auditRepo = scope.ServiceProvider.GetRequiredService<IAuditLogRepository>();

                            // Enhanced details for survey operations
                            var details = await GetEnhancedAuditDetails(context, method, path, action, userId, correlationId);

                            var auditLog = new AuditLog(userId, action, ipAddress, userAgent, details);

                            Serilog.Log.Debug("[AUDIT-MW] About to call auditRepo.AddAsync, CorrelationId: {CorrelationId}", correlationId);

                            var dbStartTime = DateTime.UtcNow;
                            Serilog.Log.Debug("[AUDIT-MW] Starting database operation at {Timestamp}, CorrelationId: {CorrelationId}", dbStartTime, correlationId);

                            await auditRepo.AddAsync(auditLog);

                            var dbEndTime = DateTime.UtcNow;
                            var dbDuration = dbEndTime - dbStartTime;
                            Serilog.Log.Information("[AUDIT-MW] Audit log saved successfully in {Duration}ms, CorrelationId: {CorrelationId}", dbDuration.TotalMilliseconds, correlationId);
                        }
                    }
                    catch (Exception ex)
                    {
                        Serilog.Log.Error(ex, "[AUDIT-MW] Failed to save audit log for action: {Action}, CorrelationId: {CorrelationId}. Continuing with request processing.",
                            action, correlationId);
                        // Don't throw exception - audit logging failure shouldn't break the request
                    }
                }
                else
                {
                    Serilog.Log.Debug("[AUDIT-MW] Skipping audit log for action: {Action}, CorrelationId: {CorrelationId}", action, correlationId);
                }

                Serilog.Log.Debug("[AUDIT-MW] About to call next middleware, CorrelationId: {CorrelationId}", correlationId);

                Serilog.Log.Debug("[AUDIT-MW] About to call _next(context), CorrelationId: {CorrelationId}", correlationId);
                if (_next != null)
                {
                    Serilog.Log.Debug("[AUDIT-MW] _next is not null, calling it, CorrelationId: {CorrelationId}", correlationId);
                    await _next(context);
                    Serilog.Log.Debug("[AUDIT-MW] _next completed, CorrelationId: {CorrelationId}", correlationId);
                }
                else
                {
                    Serilog.Log.Error("[AUDIT-MW] _next is null, cannot continue pipeline, CorrelationId: {CorrelationId}", correlationId);
                    context.Response.StatusCode = 500;
                    await context.Response.WriteAsync("Internal server error");
                }
                Serilog.Log.Information("[AUDIT-MW] Completed audit logging middleware, CorrelationId: {CorrelationId}", correlationId);
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, "[AUDIT-MW] Error in audit logging middleware for {Method} {Path}, CorrelationId: {CorrelationId}",
                    method, path, correlationId);
                throw;
            }
        }

        private string GetActionFromRequest(string path, string method)
        {
            // Authentication actions
            if (path.Contains("/api/auth/register") && method == "POST") return "UserRegistered";
            if (path.Contains("/api/auth/login") && method == "POST") return "UserLogin";
            if (path.Contains("/users/profile") && method == "PUT") return "ProfileUpdated";
            if (path.Contains("/users/request-password-reset") && method == "POST") return "PasswordResetRequested";
            if (path.Contains("/users/reset-password") && method == "POST") return "PasswordReset";

            // Survey-specific actions
            if (path.Contains("/api/surveys") || path.Contains("/api/public-surveys"))
            {
                return GetSurveyActionFromRequest(path, method);
            }

            // Add more as needed
            return null;
        }

        private string GetSurveyActionFromRequest(string path, string method)
        {
            // Public survey actions
            if (path.Contains("/api/public-surveys"))
            {
                if (method == "GET")
                {
                    if (path.Contains("/activities")) return "PublicSurveyActivitiesViewed";
                    return "PublicSurveyViewed";
                }
                else if (method == "POST")
                {
                    return "PublicSurveyVoted";
                }
            }

            // Admin survey actions
            if (path.Contains("/api/surveys"))
            {
                if (method == "GET")
                {
                    if (path.Contains("/statistics")) return "SurveyStatisticsViewed";
                    if (path.Contains("/results")) return "SurveyResultsViewed";
                    if (path.Contains("/activities")) return "SurveyActivitiesViewed";
                    return "SurveyViewed";
                }
                else if (method == "POST")
                {
                    return "SurveyCreated";
                }
                else if (method == "PUT")
                {
                    return "SurveyUpdated";
                }
                else if (method == "DELETE")
                {
                    return "SurveyDeleted";
                }
            }

            return "SurveyAction";
        }

        private async Task<string> GetEnhancedAuditDetails(HttpContext context, string method, string path, string action, Guid? userId, string correlationId)
        {
            var baseDetails = $"{method} {path}";

            // Add survey-specific details
            if (action.Contains("Survey") || action.Contains("Vote"))
            {
                var enhancedDetails = new List<string> { baseDetails };

                // Add user role information if authenticated
                if (context.User.Identity.IsAuthenticated && userId.HasValue)
                {
                    var roleClaim = context.User.FindFirst(System.Security.Claims.ClaimTypes.Role);
                    if (roleClaim != null)
                    {
                        enhancedDetails.Add($"UserRole: {roleClaim.Value}");
                    }
                }

                // Add survey-specific metadata
                if (path.Contains("/api/public-surveys"))
                {
                    enhancedDetails.Add("AccessType: Public");
                    if (method == "POST")
                    {
                        enhancedDetails.Add("OperationType: VoteSubmission");
                    }
                }
                else if (path.Contains("/api/surveys"))
                {
                    enhancedDetails.Add("AccessType: Admin/Management");

                    if (path.Contains("/statistics"))
                    {
                        enhancedDetails.Add("DataType: SurveyStatistics");
                    }
                    else if (path.Contains("/results"))
                    {
                        enhancedDetails.Add("DataType: SurveyResults");
                    }
                    else if (path.Contains("/activities"))
                    {
                        enhancedDetails.Add("DataType: SurveyActivities");
                    }
                }

                // Add query parameters for GET requests
                if (method == "GET" && context.Request.Query.Any())
                {
                    var queryParams = context.Request.Query
                        .Select(q => $"{q.Key}: {q.Value}")
                        .ToList();
                    if (queryParams.Any())
                    {
                        enhancedDetails.Add($"QueryParams: {string.Join(", ", queryParams)}");
                    }
                }

                return string.Join(" | ", enhancedDetails);
            }

            return baseDetails;
        }
    }
}