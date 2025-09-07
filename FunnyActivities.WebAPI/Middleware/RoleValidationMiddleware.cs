using System.Security.Claims;
using FunnyActivities.Domain.Entities;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace FunnyActivities.WebAPI.Middleware
{
    public class RoleValidationMiddleware
    {
        private readonly RequestDelegate _next;

        public RoleValidationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var correlationId = context.Request.Headers["X-Correlation-ID"].FirstOrDefault() ?? Guid.NewGuid().ToString();
            var path = context.Request.Path.Value;
            var method = context.Request.Method;

            Microsoft.Extensions.Logging.ILogger logger = context.RequestServices.GetRequiredService<Microsoft.Extensions.Logging.ILogger<RoleValidationMiddleware>>();
            logger.LogInformation("[ROLE-MW] Starting role validation middleware for {Method} {Path}, IsAuthenticated: {IsAuthenticated}, CorrelationId: {CorrelationId}",
                method, path, context.User.Identity?.IsAuthenticated ?? false, correlationId);

            try
            {
                if (context.User.Identity?.IsAuthenticated == true)
                {
                    logger.LogDebug("[ROLE-MW] User is authenticated, validating role, CorrelationId: {CorrelationId}", correlationId);

                    var roleClaim = context.User.FindFirst(ClaimTypes.Role);
                    if (roleClaim != null)
                    {
                        logger.LogDebug("[ROLE-MW] Found role claim: {Role}, CorrelationId: {CorrelationId}", roleClaim.Value, correlationId);

                        // Validate that the role is a valid UserRole enum value
                        if (!Enum.TryParse<UserRole>(roleClaim.Value, out _))
                        {
                            logger.LogWarning("[ROLE-MW] Invalid role in token: {Role}, CorrelationId: {CorrelationId}", roleClaim.Value, correlationId);
                            context.Response.StatusCode = StatusCodes.Status403Forbidden;
                            await context.Response.WriteAsync("Invalid role in token.");
                            return;
                        }
                        else
                        {
                            logger.LogDebug("[ROLE-MW] Role validation passed: {Role}, CorrelationId: {CorrelationId}", roleClaim.Value, correlationId);
                        }
                    }
                    else
                    {
                        logger.LogDebug("[ROLE-MW] No role claim found, CorrelationId: {CorrelationId}", correlationId);
                    }
                }
                else
                {
                    logger.LogDebug("[ROLE-MW] User is not authenticated, skipping role validation, CorrelationId: {CorrelationId}", correlationId);
                }

                logger.LogDebug("[ROLE-MW] About to call next middleware, CorrelationId: {CorrelationId}", correlationId);

                await _next(context);

                logger.LogInformation("[ROLE-MW] Completed role validation middleware, CorrelationId: {CorrelationId}", correlationId);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "[ROLE-MW] Error in role validation middleware for {Method} {Path}, CorrelationId: {CorrelationId}",
                    method, path, correlationId);
                throw;
            }
        }
    }
}