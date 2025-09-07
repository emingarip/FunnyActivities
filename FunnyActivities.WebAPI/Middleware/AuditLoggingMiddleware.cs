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
                            var details = $"{method} {path}";
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
            if (path.Contains("/api/auth/register") && method == "POST") return "UserRegistered";
            if (path.Contains("/api/auth/login") && method == "POST") return "UserLogin";
            if (path.Contains("/users/profile") && method == "PUT") return "ProfileUpdated";
            if (path.Contains("/users/request-password-reset") && method == "POST") return "PasswordResetRequested";
            if (path.Contains("/users/reset-password") && method == "POST") return "PasswordReset";
            // Add more as needed
            return null;
        }
    }
}