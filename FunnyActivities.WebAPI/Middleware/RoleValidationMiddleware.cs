using System.Security.Claims;
using FunnyActivities.Domain.Entities;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace FunnyActivities.WebAPI.Middleware
{
    public class RoleValidationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RoleValidationMiddleware> _logger;

        public RoleValidationMiddleware(RequestDelegate next, ILogger<RoleValidationMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var correlationId = context.Request.Headers["X-Correlation-ID"].FirstOrDefault() ?? Guid.NewGuid().ToString();
            var path = context.Request.Path.Value;
            var method = context.Request.Method;

            _logger.LogInformation("[ROLE-MW] Starting role validation middleware for {Method} {Path}, IsAuthenticated: {IsAuthenticated}, CorrelationId: {CorrelationId}",
                method, path, context.User.Identity?.IsAuthenticated ?? false, correlationId);

            try
            {
                if (context.User.Identity?.IsAuthenticated == true)
                {
                    _logger.LogDebug("[ROLE-MW] User is authenticated, validating role, CorrelationId: {CorrelationId}", correlationId);

                    var roleClaim = context.User.FindFirst(ClaimTypes.Role);
                    if (roleClaim != null)
                    {
                        _logger.LogDebug("[ROLE-MW] Found role claim: {Role}, CorrelationId: {CorrelationId}", roleClaim.Value, correlationId);

                        // Validate that the role is a valid UserRole enum value
                        if (!Enum.TryParse<UserRole>(roleClaim.Value, out var userRole))
                        {
                            _logger.LogWarning("[ROLE-MW] Invalid role in token: {Role}, CorrelationId: {CorrelationId}", roleClaim.Value, correlationId);
                            context.Response.StatusCode = StatusCodes.Status403Forbidden;
                            await context.Response.WriteAsync("Invalid role in token.");
                            return;
                        }

                        // Survey-specific role validation
                        if (await IsSurveyEndpoint(path))
                        {
                            var validationResult = await ValidateSurveyRoleAccess(context, userRole, path, method, correlationId);
                            if (!validationResult.IsValid)
                            {
                                _logger.LogWarning("[ROLE-MW] Survey role validation failed: {Reason}, Role: {Role}, Path: {Path}, CorrelationId: {CorrelationId}",
                                    validationResult.Reason, userRole, path, correlationId);
                                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                                await context.Response.WriteAsync(validationResult.Reason);
                                return;
                            }
                        }

                        // Enhanced admin role validation
                        if (userRole == UserRole.Admin)
                        {
                            var adminValidationResult = await ValidateAdminRole(context, path, method, correlationId);
                            if (!adminValidationResult.IsValid)
                            {
                                _logger.LogWarning("[ROLE-MW] Admin role validation failed: {Reason}, Path: {Path}, CorrelationId: {CorrelationId}",
                                    adminValidationResult.Reason, path, correlationId);
                                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                                await context.Response.WriteAsync(adminValidationResult.Reason);
                                return;
                            }
                        }

                        _logger.LogDebug("[ROLE-MW] Role validation passed: {Role}, CorrelationId: {CorrelationId}", roleClaim.Value, correlationId);
                    }
                    else
                    {
                        _logger.LogDebug("[ROLE-MW] No role claim found, CorrelationId: {CorrelationId}", correlationId);
                    }
                }
                else
                {
                    _logger.LogDebug("[ROLE-MW] User is not authenticated, skipping role validation, CorrelationId: {CorrelationId}", correlationId);
                }

                _logger.LogDebug("[ROLE-MW] About to call next middleware, CorrelationId: {CorrelationId}", correlationId);

                await _next(context);

                _logger.LogInformation("[ROLE-MW] Completed role validation middleware, CorrelationId: {CorrelationId}", correlationId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[ROLE-MW] Error in role validation middleware for {Method} {Path}, CorrelationId: {CorrelationId}",
                    method, path, correlationId);
                throw;
            }
        }

        private async Task<bool> IsSurveyEndpoint(string path)
        {
            return path.Contains("/api/surveys") || path.Contains("/api/public-surveys");
        }

        private async Task<(bool IsValid, string Reason)> ValidateSurveyRoleAccess(HttpContext context, UserRole userRole, string path, string method, string correlationId)
        {
            // Public survey endpoints - allow all authenticated users
            if (path.Contains("/api/public-surveys") && (method == "GET" || method == "POST"))
            {
                return (true, "Public survey access granted");
            }

            // Admin-only survey operations
            if (method == "DELETE" || path.Contains("/admin") || path.Contains("/statistics"))
            {
                if (userRole != UserRole.Admin)
                {
                    return (false, "Admin role required for this survey operation");
                }
                return (true, "Admin survey access granted");
            }

            // Survey management operations - Admin and Viewer roles
            if (path.Contains("/api/surveys") && (method == "PUT" || method == "POST"))
            {
                if (userRole != UserRole.Admin && userRole != UserRole.Viewer)
                {
                    return (false, "Admin or Viewer role required for survey management");
                }
                return (true, "Survey management access granted");
            }

            // Default: allow access for authenticated users
            return (true, "Default survey access granted");
        }

        private async Task<(bool IsValid, string Reason)> ValidateAdminRole(HttpContext context, string path, string method, string correlationId)
        {
            // Enhanced admin validation for sensitive operations
            var sensitivePaths = new[] { "/api/users", "/api/roles", "/api/admin", "/api/surveys/admin" };

            foreach (var sensitivePath in sensitivePaths)
            {
                if (path.Contains(sensitivePath))
                {
                    _logger.LogDebug("[ROLE-MW] Admin accessing sensitive path: {Path}, CorrelationId: {CorrelationId}", path, correlationId);

                    // Additional validation could be added here (e.g., check admin permissions, IP restrictions, etc.)
                    return (true, "Admin access to sensitive path validated");
                }
            }

            return (true, "Admin role validation passed");
        }
    }
}