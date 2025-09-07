using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace FunnyActivities.WebAPI.Middleware
{
    /// <summary>
    /// Middleware for centralized user authentication and context management.
    /// Validates user identity and stores user information in HttpContext.Items for easy access.
    /// </summary>
    public class AuthenticationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<AuthenticationMiddleware> _logger;

        public AuthenticationMiddleware(RequestDelegate next, ILogger<AuthenticationMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        /// <summary>
        /// Processes the HTTP request to validate user authentication and set user context.
        /// </summary>
        /// <param name="context">The HTTP context for the current request.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task InvokeAsync(HttpContext context)
        {
            var correlationId = context.Request.Headers["X-Correlation-ID"].FirstOrDefault() ?? Guid.NewGuid().ToString();
            var path = context.Request.Path.Value;
            var method = context.Request.Method;

            _logger.LogInformation("[AUTH-MW] Starting authentication middleware for {Method} {Path}, IsAuthenticated: {IsAuthenticated}, CorrelationId: {CorrelationId}",
                method, path, context.User.Identity?.IsAuthenticated ?? false, correlationId);

            try
            {
                if (context.User.Identity?.IsAuthenticated == true)
                {
                    _logger.LogDebug("[AUTH-MW] User is authenticated, extracting user ID, CorrelationId: {CorrelationId}", correlationId);

                    var userId = ExtractUserId(context.User);
                    if (userId != Guid.Empty)
                    {
                        // Store user context in HttpContext.Items for easy access by controllers
                        context.Items["UserId"] = userId;
                        context.Items["UserRole"] = context.User.FindFirst(ClaimTypes.Role)?.Value;
                        context.Items["UserName"] = context.User.Identity.Name;

                        _logger.LogInformation("[AUTH-MW] User context set for user {UserId}, CorrelationId: {CorrelationId}",
                            userId, correlationId);
                    }
                    else
                    {
                        _logger.LogWarning("[AUTH-MW] Authenticated user has invalid user ID claim, CorrelationId: {CorrelationId}", correlationId);
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        await context.Response.WriteAsJsonAsync(new
                        {
                            error = "Invalid user identity",
                            message = "User ID claim is missing or invalid"
                        });
                        return;
                    }
                }
                else
                {
                    _logger.LogDebug("[AUTH-MW] User is not authenticated, proceeding without user context, CorrelationId: {CorrelationId}", correlationId);
                }

                _logger.LogDebug("[AUTH-MW] About to call next middleware, CorrelationId: {CorrelationId}", correlationId);

                await _next(context);

                _logger.LogInformation("[AUTH-MW] Completed authentication middleware, CorrelationId: {CorrelationId}", correlationId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[AUTH-MW] Error in authentication middleware for {Method} {Path}, CorrelationId: {CorrelationId}",
                    method, path, correlationId);
                throw;
            }
        }

        /// <summary>
        /// Extracts the user ID from the claims principal.
        /// </summary>
        /// <param name="user">The claims principal containing user information.</param>
        /// <returns>The user ID if valid, otherwise Guid.Empty.</returns>
        private Guid ExtractUserId(ClaimsPrincipal user)
        {
            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId))
            {
                return userId;
            }

            _logger.LogWarning("Failed to extract valid user ID from claims. Claim value: {Value}",
                userIdClaim?.Value ?? "null");
            return Guid.Empty;
        }
    }
}