using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace FunnyActivities.WebAPI.Controllers.Base
{
    /// <summary>
    /// Base controller providing centralized access to authenticated user context.
    /// All controllers should inherit from this base class to access user information.
    /// </summary>
    [Authorize]
    public abstract class BaseController : ControllerBase
    {
        protected readonly ILogger _logger;

        protected BaseController(ILogger logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Gets the current authenticated user's ID.
        /// This property is guaranteed to return a valid user ID when accessed from authenticated endpoints.
        /// </summary>
        protected Guid CurrentUserId
        {
            get
            {
                if (HttpContext.Items.TryGetValue("UserId", out var userIdObj) && userIdObj is Guid userId)
                {
                    return userId;
                }

                // Fallback extraction (should not happen if middleware is working correctly)
                _logger.LogWarning("UserId not found in HttpContext.Items, falling back to claim extraction");
                var extractedUserId = ExtractUserId();
                HttpContext.Items["UserId"] = extractedUserId;
                return extractedUserId;
            }
        }

        /// <summary>
        /// Gets the current authenticated user's role.
        /// </summary>
        protected string? CurrentUserRole
        {
            get
            {
                if (HttpContext.Items.TryGetValue("UserRole", out var userRoleObj))
                {
                    return userRoleObj as string;
                }

                // Fallback extraction
                var role = User.FindFirst(ClaimTypes.Role)?.Value;
                HttpContext.Items["UserRole"] = role;
                return role;
            }
        }

        /// <summary>
        /// Gets the current authenticated user's name.
        /// </summary>
        protected string? CurrentUserName
        {
            get
            {
                if (HttpContext.Items.TryGetValue("UserName", out var userNameObj))
                {
                    return userNameObj as string;
                }

                // Fallback extraction
                var userName = User.Identity?.Name;
                HttpContext.Items["UserName"] = userName;
                return userName;
            }
        }

        /// <summary>
        /// Validates that the current user has the specified role.
        /// </summary>
        /// <param name="requiredRole">The role required for the operation.</param>
        /// <returns>True if the user has the required role, false otherwise.</returns>
        protected bool HasRole(string requiredRole)
        {
            var userRole = CurrentUserRole;
            return string.Equals(userRole, requiredRole, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Validates that the current user has any of the specified roles.
        /// </summary>
        /// <param name="requiredRoles">The roles to check against.</param>
        /// <returns>True if the user has any of the required roles, false otherwise.</returns>
        protected bool HasAnyRole(params string[] requiredRoles)
        {
            var userRole = CurrentUserRole;
            return requiredRoles.Any(role =>
                string.Equals(userRole, role, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Extracts the user ID from the current user's claims.
        /// This is a fallback method and should not be called directly in normal operation.
        /// </summary>
        /// <returns>The user ID if valid, otherwise throws UnauthorizedAccessException.</returns>
        /// <exception cref="UnauthorizedAccessException">Thrown when user ID cannot be extracted.</exception>
        private Guid ExtractUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId))
            {
                return userId;
            }

            _logger.LogError("Failed to extract valid user ID from claims for authenticated user");
            throw new UnauthorizedAccessException("Invalid user identity");
        }
    }
}