# Centralized Authentication and Authorization Design

## Current Implementation Analysis

### Existing Pattern
The current implementation has repetitive authentication logic across controllers:

```csharp
private Guid GetCurrentUserId()
{
    var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
    var roleClaim = User.FindFirst(ClaimTypes.Role);
    var customRoleClaim = User.FindFirst("role");

    // Extensive logging for debugging
    var allClaims = User.Claims.Select(c => $"{c.Type}: {c.Value}").ToList();
    _logger.LogInformation("All user claims: {Claims}", string.Join(", ", allClaims));

    // More logging...
    if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId))
    {
        return userId;
    }

    return Guid.Empty;
}
```

Each write operation includes:
```csharp
var userId = GetCurrentUserId();
if (userId == Guid.Empty)
{
    return this.ApiError("User not authenticated", "Unauthorized", 401);
}
```

### Issues with Current Approach
1. **Code Duplication**: Same logic repeated in every controller
2. **Maintenance Burden**: Changes require updates across multiple files
3. **Inconsistent Error Handling**: Different controllers may handle auth failures differently
4. **Performance Overhead**: Extensive logging on every request
5. **Testing Complexity**: Authentication logic mixed with business logic

## Recommended Centralized Approaches

### Approach 1: Authentication Middleware (Recommended)

**Best for**: Global authentication handling with centralized user context management.

#### Implementation

```csharp
// FunnyActivities.WebAPI/Middleware/AuthenticationMiddleware.cs
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace FunnyActivities.WebAPI.Middleware
{
    public class AuthenticationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<AuthenticationMiddleware> _logger;

        public AuthenticationMiddleware(RequestDelegate next, ILogger<AuthenticationMiddleware> _logger)
        {
            _next = next;
            this._logger = _logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.User.Identity?.IsAuthenticated == true)
            {
                var userId = ExtractUserId(context.User);
                if (userId != Guid.Empty)
                {
                    // Store user ID in HttpContext.Items for easy access
                    context.Items["UserId"] = userId;
                    context.Items["UserRole"] = context.User.FindFirst(ClaimTypes.Role)?.Value;
                }
                else
                {
                    _logger.LogWarning("Authenticated user has invalid user ID claim");
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await context.Response.WriteAsJsonAsync(new { error = "Invalid user identity" });
                    return;
                }
            }

            await _next(context);
        }

        private Guid ExtractUserId(ClaimsPrincipal user)
        {
            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier);
            return userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId)
                ? userId
                : Guid.Empty;
        }
    }
}
```

```csharp
// FunnyActivities.WebAPI/Extensions/MiddlewareExtensions.cs
namespace FunnyActivities.WebAPI.Extensions
{
    public static class MiddlewareExtensions
    {
        public static IApplicationBuilder UseAuthenticationMiddleware(this IApplicationBuilder app)
        {
            return app.UseMiddleware<AuthenticationMiddleware>();
        }
    }
}
```

```csharp
// Program.cs - Add middleware registration
app.UseAuthentication(); // Existing JWT middleware
app.UseAuthorization();
app.UseAuthenticationMiddleware(); // Our new middleware
```

#### Controller Usage

```csharp
// FunnyActivities.WebAPI/Controllers/MaterialsController.cs
[ApiController]
[Route("api/materials")]
[Authorize]
public class MaterialsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<MaterialsController> _logger;

    public MaterialsController(IMediator mediator, ILogger<MaterialsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [HttpPost]
    [Authorize(Policy = "CanCreateMaterial")]
    public async Task<IActionResult> CreateMaterial([FromBody] CreateMaterialWrapper wrapper)
    {
        var request = wrapper.Request;
        _logger.LogInformation("Creating material: {Name}", request.Name);

        // User ID is guaranteed to be valid due to middleware
        var userId = (Guid)HttpContext.Items["UserId"];

        var command = new CreateMaterialCommand
        {
            Name = request.Name,
            // ... other properties
            UserId = userId
        };

        // No authentication check needed - handled by middleware
        var result = await _mediator.Send(command);
        return this.ApiCreated(nameof(GetMaterial), new { id = result.Id }, result, "Material created successfully");
    }
}
```

### Approach 2: Authorization Filter

**Best for**: Controller-level authentication with fine-grained control.

```csharp
// FunnyActivities.CrossCuttingConcerns/Authorization/UserAuthenticationFilter.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace FunnyActivities.CrossCuttingConcerns.Authorization
{
    public class UserAuthenticationFilter : IAuthorizationFilter
    {
        private readonly ILogger<UserAuthenticationFilter> _logger;

        public UserAuthenticationFilter(ILogger<UserAuthenticationFilter> logger)
        {
            _logger = logger;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            if (!context.HttpContext.User.Identity.IsAuthenticated)
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            var userId = ExtractUserId(context.HttpContext.User);
            if (userId == Guid.Empty)
            {
                _logger.LogWarning("User authentication failed - invalid user ID");
                context.Result = new ObjectResult(new { error = "Invalid user identity" })
                {
                    StatusCode = StatusCodes.Status401Unauthorized
                };
                return;
            }

            // Store user ID for controller access
            context.HttpContext.Items["UserId"] = userId;
        }

        private Guid ExtractUserId(ClaimsPrincipal user)
        {
            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier);
            return userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId)
                ? userId
                : Guid.Empty;
        }
    }
}
```

```csharp
// FunnyActivities.WebAPI/Controllers/MaterialsController.cs
[ApiController]
[Route("api/materials")]
[Authorize]
[ServiceFilter(typeof(UserAuthenticationFilter))] // Applied to all actions
public class MaterialsController : ControllerBase
{
    // Controller implementation - user ID guaranteed to be available
    [HttpPost]
    [Authorize(Policy = "CanCreateMaterial")]
    public async Task<IActionResult> CreateMaterial([FromBody] CreateMaterialWrapper wrapper)
    {
        var userId = (Guid)HttpContext.Items["UserId"];
        // No authentication checks needed
    }
}
```

### Approach 3: Base Controller Class

**Best for**: Simple inheritance-based approach with common authentication logic.

```csharp
// FunnyActivities.WebAPI/Controllers/Base/BaseController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace FunnyActivities.WebAPI.Controllers.Base
{
    [Authorize]
    public abstract class BaseController : ControllerBase
    {
        protected readonly ILogger _logger;

        protected BaseController(ILogger logger)
        {
            _logger = logger;
        }

        protected Guid CurrentUserId
        {
            get
            {
                if (HttpContext.Items.ContainsKey("UserId"))
                {
                    return (Guid)HttpContext.Items["UserId"];
                }

                var userId = ExtractUserId();
                HttpContext.Items["UserId"] = userId;
                return userId;
            }
        }

        protected string CurrentUserRole => HttpContext.Items["UserRole"] as string;

        private Guid ExtractUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId))
            {
                return userId;
            }

            _logger.LogWarning("Failed to extract valid user ID from claims");
            throw new UnauthorizedAccessException("Invalid user identity");
        }
    }
}
```

```csharp
// FunnyActivities.WebAPI/Controllers/MaterialsController.cs
using FunnyActivities.WebAPI.Controllers.Base;

public class MaterialsController : BaseController
{
    private readonly IMediator _mediator;

    public MaterialsController(IMediator mediator, ILogger<MaterialsController> logger)
        : base(logger)
    {
        _mediator = mediator;
    }

    [HttpPost]
    [Authorize(Policy = "CanCreateMaterial")]
    public async Task<IActionResult> CreateMaterial([FromBody] CreateMaterialWrapper wrapper)
    {
        var request = wrapper.Request;
        _logger.LogInformation("Creating material: {Name}", request.Name);

        // User ID is available as property - no manual extraction needed
        var command = new CreateMaterialCommand
        {
            Name = request.Name,
            UserId = CurrentUserId // Clean, simple access
        };

        var result = await _mediator.Send(command);
        return this.ApiCreated(nameof(GetMaterial), new { id = result.Id }, result, "Material created successfully");
    }
}
```

### Approach 4: Custom Authorization Policy with User Context

**Best for**: Policy-based authorization with automatic user context injection.

```csharp
// FunnyActivities.CrossCuttingConcerns/Authorization/UserContextRequirement.cs
using Microsoft.AspNetCore.Authorization;

namespace FunnyActivities.CrossCuttingConcerns.Authorization
{
    public class UserContextRequirement : IAuthorizationRequirement
    {
        public string PolicyName { get; }

        public UserContextRequirement(string policyName)
        {
            PolicyName = policyName;
        }
    }
}
```

```csharp
// FunnyActivities.CrossCuttingConcerns/Authorization/UserContextHandler.cs
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace FunnyActivities.CrossCuttingConcerns.Authorization
{
    public class UserContextHandler : AuthorizationHandler<UserContextRequirement>
    {
        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            UserContextRequirement requirement)
        {
            if (context.User.Identity?.IsAuthenticated != true)
            {
                return Task.CompletedTask;
            }

            var userId = ExtractUserId(context.User);
            if (userId == Guid.Empty)
            {
                return Task.CompletedTask;
            }

            // Store user context for the request
            if (context.Resource is HttpContext httpContext)
            {
                httpContext.Items["UserId"] = userId;
                httpContext.Items["UserRole"] = context.User.FindFirst(ClaimTypes.Role)?.Value;
            }

            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        private Guid ExtractUserId(ClaimsPrincipal user)
        {
            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier);
            return userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId)
                ? userId
                : Guid.Empty;
        }
    }
}
```

```csharp
// ServiceCollectionExtensions.cs - Update authorization policies
public static IServiceCollection AddAuthorizationPolicies(this IServiceCollection services)
{
    services.AddAuthorization(options =>
    {
        // Enhanced policies with user context
        options.AddPolicy("CanCreateMaterial", policy =>
        {
            policy.RequireAuthenticatedUser();
            policy.RequireRole("Admin");
            policy.Requirements.Add(new UserContextRequirement("CanCreateMaterial"));
        });

        // Add other policies similarly...
    });

    services.AddScoped<IAuthorizationHandler, UserContextHandler>();
    return services;
}
```

## Security Best Practices

### 1. Defense in Depth
- **Multiple Layers**: Combine middleware, filters, and policies
- **Fail-Safe Defaults**: Default to denying access when in doubt
- **Principle of Least Privilege**: Grant minimum required permissions

### 2. Input Validation
```csharp
// Validate user ID format and range
private bool IsValidUserId(Guid userId)
{
    return userId != Guid.Empty && userId != default;
}
```

### 3. Secure Logging
```csharp
// Avoid logging sensitive information
_logger.LogInformation("User {UserId} performed action", userId);
// DON'T: _logger.LogInformation("User {UserId} with token {Token}", userId, jwtToken);
```

### 4. Rate Limiting
```csharp
// Implement rate limiting for authentication endpoints
[HttpPost("login")]
[EnableRateLimiting("LoginPolicy")]
public async Task<IActionResult> Login([FromBody] LoginRequest request)
{
    // Implementation
}
```

### 5. Token Security
- Use short-lived access tokens (15-30 minutes)
- Implement refresh token rotation
- Validate token expiration and issuer
- Use HTTPS for all authenticated endpoints

## Performance Considerations

### Approach Performance Comparison

| Approach | Startup Impact | Runtime Impact | Memory Usage | Maintainability |
|----------|----------------|----------------|--------------|----------------|
| Middleware | Low | Very Low | Low | High |
| Filter | Low | Low | Low | High |
| Base Controller | None | Low | Low | High |
| Policy Handler | Low | Medium | Low | High |

### Optimization Strategies

1. **Caching User Context**
```csharp
// Cache user context per request
private readonly ConcurrentDictionary<string, UserContext> _userCache = new();

public UserContext GetUserContext(HttpContext context)
{
    var cacheKey = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    if (string.IsNullOrEmpty(cacheKey)) return null;

    return _userCache.GetOrAdd(cacheKey, _ => ExtractUserContext(context.User));
}
```

2. **Async Authentication**
```csharp
public async Task<Guid> GetCurrentUserIdAsync(HttpContext context)
{
    if (context.Items.ContainsKey("UserId"))
    {
        return (Guid)context.Items["UserId"];
    }

    // Async validation if needed
    var userId = await ValidateAndExtractUserIdAsync(context.User);
    context.Items["UserId"] = userId;
    return userId;
}
```

## Migration Strategy

### Phase 1: Infrastructure Setup
1. Implement chosen approach (recommend Middleware + Base Controller)
2. Add comprehensive logging and monitoring
3. Create migration utilities

### Phase 2: Gradual Migration
1. Start with low-risk controllers
2. Update one controller at a time
3. Run integration tests after each migration
4. Monitor performance metrics

### Phase 3: Cleanup
1. Remove old GetCurrentUserId() methods
2. Update documentation
3. Remove duplicate authentication logic

### Migration Example for MaterialsController

```csharp
// BEFORE (current implementation)
[HttpPost]
[Authorize(Policy = "CanCreateMaterial")]
public async Task<IActionResult> CreateMaterial([FromBody] CreateMaterialWrapper wrapper)
{
    var request = wrapper.Request;
    _logger.LogInformation("Creating material: {Name}", request.Name);

    var userId = GetCurrentUserId(); // OLD METHOD
    if (userId == Guid.Empty)
    {
        return this.ApiError("User not authenticated", "Unauthorized", 401);
    }

    var command = new CreateMaterialCommand
    {
        // ... properties
        UserId = userId
    };

    // ... rest of method
}

// AFTER (centralized approach)
[HttpPost]
[Authorize(Policy = "CanCreateMaterial")]
public async Task<IActionResult> CreateMaterial([FromBody] CreateMaterialWrapper wrapper)
{
    var request = wrapper.Request;
    _logger.LogInformation("Creating material: {Name}", request.Name);

    // User ID automatically available and validated
    var command = new CreateMaterialCommand
    {
        // ... properties
        UserId = CurrentUserId // From BaseController
    };

    // ... rest of method - no auth checks needed
}
```

## Trade-offs Analysis

### Middleware Approach
**Pros:**
- Global handling, no controller changes needed
- Early validation prevents unnecessary processing
- Centralized logging and monitoring

**Cons:**
- All requests go through middleware (even static files)
- Less granular control per controller
- Harder to customize per endpoint

### Filter Approach
**Pros:**
- Applied per controller or action
- Easy to customize behavior
- Good separation of concerns

**Cons:**
- Multiple filters can be complex
- Order of execution matters
- More boilerplate code

### Base Controller Approach
**Pros:**
- Clean inheritance model
- Easy property access
- Familiar OOP pattern

**Cons:**
- Requires inheritance (can't use with existing base classes)
- All controllers must inherit from base
- Less flexible for different auth patterns

### Policy Handler Approach
**Pros:**
- Leverages ASP.NET Core authorization framework
- Declarative configuration
- Easy to test and extend

**Cons:**
- More complex setup
- Authorization logic mixed with business logic
- Harder to debug complex policy chains

## Recommended Implementation Strategy

1. **Primary**: Use **Middleware + Base Controller** combination
2. **Secondary**: Enhance with **Custom Authorization Policies**
3. **Fallback**: Use **Authorization Filters** for specific cases

This approach provides:
- Global authentication validation
- Clean controller code
- Flexible authorization policies
- Easy testing and maintenance
- Good performance characteristics

## Testing Strategy

### Unit Tests
```csharp
[Test]
public async Task CreateMaterial_WithValidUser_CreatesSuccessfully()
{
    // Arrange
    var mockHttpContext = new Mock<HttpContext>();
    mockHttpContext.Setup(c => c.Items).Returns(new Dictionary<object, object>
    {
        ["UserId"] = Guid.NewGuid()
    });

    var controller = new MaterialsController(mockMediator.Object, mockLogger.Object)
    {
        ControllerContext = new ControllerContext
        {
            HttpContext = mockHttpContext.Object
        }
    };

    // Act & Assert
    // Test that user ID is properly extracted and used
}
```

### Integration Tests
```csharp
[Test]
public async Task AuthenticationMiddleware_ValidToken_SetsUserContext()
{
    // Arrange
    var client = _factory.CreateClient();
    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", validToken);

    // Act
    var response = await client.PostAsync("/api/materials", content);

    // Assert
    response.EnsureSuccessStatusCode();
    // Verify user context was properly set and used
}
```

This design provides a robust, maintainable, and secure authentication and authorization system that eliminates code duplication while maintaining the current security level.