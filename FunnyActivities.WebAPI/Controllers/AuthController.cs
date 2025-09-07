using MediatR;
using Microsoft.AspNetCore.Mvc;
using FunnyActivities.Application.Commands.UserManagement;
using FunnyActivities.Application.DTOs.UserManagement;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System;

namespace FunnyActivities.WebAPI.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IMediator mediator, ILogger<AuthController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterUserRequest request)
        {
            var stopwatch = Stopwatch.StartNew();
            var correlationId = GetCorrelationId();
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var method = Request.Method;
            var path = Request.Path.ToString();

            _logger.LogInformation("[AUTH-REG] Register attempt", new { Email = MaskEmail(request.Email), IP = MaskIP(ip), Method = method, Path = path, CorrelationId = correlationId });

            try
            {
                var command = new RegisterUserCommand
                {
                    Email = request.Email,
                    Password = request.Password,
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    Role = request.Role
                };

                _logger.LogInformation("[AUTH-REG] Before user registration", new { Email = MaskEmail(request.Email), IP = MaskIP(ip), Method = method, Path = path, CorrelationId = correlationId });

                await _mediator.Send(command);

                _logger.LogInformation("[AUTH-REG] User registration successful", new { Email = MaskEmail(request.Email), IP = MaskIP(ip), Method = method, Path = path, CorrelationId = correlationId });

                // After registration, automatically log in the user
                var loginCommand = new LoginUserCommand
                {
                    Email = request.Email,
                    Password = request.Password
                };

                _logger.LogInformation("[AUTH-REG] Before automatic login", new { Email = MaskEmail(request.Email), IP = MaskIP(ip), Method = method, Path = path, CorrelationId = correlationId });

                var loginResult = await _mediator.Send(loginCommand);

                _logger.LogInformation("[AUTH-REG] Automatic login successful", new { UserId = loginResult.User.Id, Email = MaskEmail(request.Email), IP = MaskIP(ip), Method = method, Path = path, CorrelationId = correlationId });

                stopwatch.Stop();
                _logger.LogInformation("[AUTH-REG] Register and login completed", new { UserId = loginResult.User.Id, Email = MaskEmail(request.Email), IP = MaskIP(ip), Method = method, Path = path, Status = 200, DurationMs = stopwatch.ElapsedMilliseconds, CorrelationId = correlationId });

                var data = new
                {
                    user = loginResult.User,
                    accessToken = loginResult.Token,
                    refreshToken = loginResult.RefreshToken
                };

                return this.ApiSuccess(data, "Registration successful");
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "[AUTH-REG] Register failed", new { Email = MaskEmail(request.Email), Error = ex.Message, IP = MaskIP(ip), Method = method, Path = path, Status = 400, DurationMs = stopwatch.ElapsedMilliseconds, CorrelationId = correlationId });

                // Check if it's a conflict (user already exists)
                if (ex.Message.Contains("already exists", StringComparison.OrdinalIgnoreCase) ||
                    ex.Message.Contains("duplicate", StringComparison.OrdinalIgnoreCase))
                {
                    return this.ApiError("User already exists", "ConflictError", 409);
                }

                return this.ApiError(ex.Message, "ValidationError", 400);
            }
        }

        [HttpGet("test")]
        [AllowAnonymous]
        public IActionResult Test()
        {
            _logger.LogInformation("[AUTH-TEST] Test endpoint called successfully");
            return Ok(new { message = "Test endpoint working", timestamp = DateTime.UtcNow });
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var stopwatch = Stopwatch.StartNew();
            var correlationId = GetCorrelationId();
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var method = Request.Method;
            var path = Request.Path.ToString();

            _logger.LogInformation("[AUTH-LOGIN] Login attempt", new { Email = MaskEmail(request.Email), IP = MaskIP(ip), Method = method, Path = path, CorrelationId = correlationId });

            try
            {
                _logger.LogDebug("[AUTH-LOGIN] Creating LoginUserCommand", new { Email = MaskEmail(request.Email), IP = MaskIP(ip), Method = method, Path = path, CorrelationId = correlationId });

                var command = new LoginUserCommand
                {
                    Email = request.Email,
                    Password = request.Password
                };

                _logger.LogInformation("[AUTH-LOGIN] Before user login", new { Email = MaskEmail(request.Email), IP = MaskIP(ip), Method = method, Path = path, CorrelationId = correlationId });

                _logger.LogDebug("[AUTH-LOGIN] About to call _mediator.Send", new { Email = MaskEmail(request.Email), IP = MaskIP(ip), Method = method, Path = path, CorrelationId = correlationId });

                var result = await _mediator.Send(command);

                _logger.LogInformation("[AUTH-LOGIN] Login successful", new { UserId = result.User.Id, Email = MaskEmail(request.Email), IP = MaskIP(ip), Method = method, Path = path, CorrelationId = correlationId });

                _logger.LogDebug("[AUTH-LOGIN] About to create response data", new { UserId = result.User.Id, Email = MaskEmail(request.Email), IP = MaskIP(ip), Method = method, Path = path, CorrelationId = correlationId });

                stopwatch.Stop();
                _logger.LogInformation("[AUTH-LOGIN] Login completed", new { UserId = result.User.Id, Email = MaskEmail(request.Email), IP = MaskIP(ip), Method = method, Path = path, Status = 200, DurationMs = stopwatch.ElapsedMilliseconds, CorrelationId = correlationId });

                var data = new
                {
                    user = result.User,
                    accessToken = result.Token,
                    refreshToken = result.RefreshToken
                };

                _logger.LogDebug("[AUTH-LOGIN] About to return ApiSuccess", new { UserId = result.User.Id, Email = MaskEmail(request.Email), IP = MaskIP(ip), Method = method, Path = path, CorrelationId = correlationId });

                return this.ApiSuccess(data, "Login successful");
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "[AUTH-LOGIN] Login failed", new { Email = MaskEmail(request.Email), Error = ex.Message, IP = MaskIP(ip), Method = method, Path = path, Status = 401, DurationMs = stopwatch.ElapsedMilliseconds, CorrelationId = correlationId });

                _logger.LogDebug("[AUTH-LOGIN] About to return ApiError", new { Email = MaskEmail(request.Email), Error = ex.Message, IP = MaskIP(ip), Method = method, Path = path, CorrelationId = correlationId });

                return this.ApiError("Invalid credentials", "AuthenticationError", 401);
            }
        }

        [HttpPost("refresh")]
        [AllowAnonymous]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            var stopwatch = Stopwatch.StartNew();
            var correlationId = GetCorrelationId();
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var method = Request.Method;
            var path = Request.Path.ToString();

            _logger.LogInformation("[AUTH-REFRESH] Token refresh attempt", new { IP = MaskIP(ip), Method = method, Path = path, CorrelationId = correlationId });

            try
            {
                var command = new RefreshTokenCommand
                {
                    RefreshToken = request.RefreshToken,
                    AccessToken = request.AccessToken
                };

                _logger.LogInformation("[AUTH-REFRESH] Before token refresh", new { IP = MaskIP(ip), Method = method, Path = path, CorrelationId = correlationId });

                var result = await _mediator.Send(command);

                _logger.LogInformation("[AUTH-REFRESH] Token refresh successful", new { UserId = result.User.Id, IP = MaskIP(ip), Method = method, Path = path, CorrelationId = correlationId });

                stopwatch.Stop();
                _logger.LogInformation("[AUTH-REFRESH] Token refresh completed", new { UserId = result.User.Id, IP = MaskIP(ip), Method = method, Path = path, Status = 200, DurationMs = stopwatch.ElapsedMilliseconds, CorrelationId = correlationId });

                var data = new
                {
                    accessToken = result.Token,
                    refreshToken = result.RefreshToken,
                    user = result.User
                };

                return this.ApiSuccess(data, "Token refreshed");
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "[AUTH-REFRESH] Token refresh failed", new { Error = ex.Message, IP = MaskIP(ip), Method = method, Path = path, Status = 401, DurationMs = stopwatch.ElapsedMilliseconds, CorrelationId = correlationId });

                return this.ApiError(ex.Message, "AuthenticationError", 401);
            }
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            var stopwatch = Stopwatch.StartNew();
            var correlationId = GetCorrelationId();
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var method = Request.Method;
            var path = Request.Path.ToString();
            var userId = User.FindFirst("sub")?.Value ?? User.Identity.Name ?? "unknown";

            _logger.LogInformation("[AUTH-LOGOUT] Logout attempt", new { UserId = userId, IP = MaskIP(ip), Method = method, Path = path, CorrelationId = correlationId });

            try
            {
                // In a stateless JWT system, logout is handled client-side
                // But we can log the logout event if needed
                stopwatch.Stop();
                _logger.LogInformation("[AUTH-LOGOUT] Logout successful", new { UserId = userId, IP = MaskIP(ip), Method = method, Path = path, Status = 200, DurationMs = stopwatch.ElapsedMilliseconds, CorrelationId = correlationId });

                return this.ApiSuccess("Logged out successfully");
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "[AUTH-LOGOUT] Logout failed", new { UserId = userId, Error = ex.Message, IP = MaskIP(ip), Method = method, Path = path, Status = 400, DurationMs = stopwatch.ElapsedMilliseconds, CorrelationId = correlationId });

                return this.ApiError(ex.Message, "Error", 400);
            }
        }

        // Helper methods for PII-safe logging
        private string MaskEmail(string email)
        {
            if (string.IsNullOrEmpty(email)) return "***";
            var atIndex = email.IndexOf('@');
            if (atIndex > 3)
                return email.Substring(0, 3) + "***" + email.Substring(atIndex);
            return "***" + email.Substring(atIndex);
        }

        private string MaskIP(string ip)
        {
            if (string.IsNullOrEmpty(ip) || ip == "unknown") return ip;
            var parts = ip.Split('.');
            if (parts.Length >= 1)
                return parts[0] + ".***.***.***";
            return "***.***.***.***";
        }

        private string GetCorrelationId()
        {
            // Try to get from header, otherwise generate
            if (HttpContext.Request.Headers.TryGetValue("X-Correlation-ID", out var correlationId) && !string.IsNullOrWhiteSpace(correlationId))
                return correlationId.ToString();
            return Guid.NewGuid().ToString();
        }
    }

    public class RefreshTokenRequest
    {
        public string RefreshToken { get; set; }
        public string AccessToken { get; set; }
    }
}