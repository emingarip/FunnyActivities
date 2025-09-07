using FluentAssertions;
using FunnyActivities.CrossCuttingConcerns.ErrorHandling;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using System.Net;
using System.Text.Json;
using Xunit;

namespace FunnyActivities.Application.UnitTests.ErrorHandling
{
    /// <summary>
    /// Unit tests for ExceptionMiddleware.
    /// Tests exception handling, HTTP status codes, ProblemDetails format,
    /// and logging for various exception types.
    /// </summary>
    public class ExceptionMiddlewareTests
    {
        private readonly Mock<ILogger<ExceptionMiddleware>> _loggerMock;
        private readonly ExceptionMiddleware _middleware;
        private readonly Mock<RequestDelegate> _nextMock;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExceptionMiddlewareTests"/> class.
        /// </summary>
        public ExceptionMiddlewareTests()
        {
            _loggerMock = new Mock<ILogger<ExceptionMiddleware>>();
            _nextMock = new Mock<RequestDelegate>();
            _middleware = new ExceptionMiddleware(_nextMock.Object, _loggerMock.Object);
        }

        #region Successful Request Processing

        [Fact]
        public async Task InvokeAsync_NoException_ShouldContinuePipeline()
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            var nextCalled = false;

            _nextMock.Setup(x => x(It.IsAny<HttpContext>()))
                .Callback(() => nextCalled = true)
                .Returns(Task.CompletedTask);

            // Act
            await _middleware.InvokeAsync(httpContext);

            // Assert
            nextCalled.Should().BeTrue();
            _nextMock.Verify(x => x(httpContext), Times.Once);
        }

        #endregion

        #region ValidationException Handling

        [Fact]
        public async Task InvokeAsync_ValidationException_ShouldReturnBadRequestWithProblemDetails()
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            httpContext.Response.Body = new MemoryStream();
            var validationException = new ValidationException(new[] { "Name is required", "Stock must be positive" });

            _nextMock.Setup(x => x(It.IsAny<HttpContext>()))
                .ThrowsAsync(validationException);

            // Act
            await _middleware.InvokeAsync(httpContext);

            // Assert
            httpContext.Response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
            httpContext.Response.ContentType.Should().Be("application/problem+json");

            var responseBody = await ReadResponseBody(httpContext.Response);
            var problemDetails = JsonSerializer.Deserialize<ProblemDetails>(responseBody);

            problemDetails.Should().NotBeNull();
            problemDetails.Title.Should().Be("Validation Error");
            problemDetails.Status.Should().Be(400);
            problemDetails.Detail.Should().Be("One or more validation errors occurred.");
            problemDetails.Extensions.Should().ContainKey("errors");
            problemDetails.Extensions.Should().ContainKey("errorCode");
            problemDetails.Extensions["errorCode"].Should().Be("VALIDATION_FAILED");

            var errors = (IEnumerable<string>)problemDetails.Extensions["errors"];
            errors.Should().BeEquivalentTo(new[] { "Name is required", "Stock must be positive" });
        }

        [Fact]
        public async Task InvokeAsync_ValidationException_ShouldLogWarning()
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Method = "POST";
            httpContext.Request.Path = "/api/materials";
            httpContext.User = new System.Security.Claims.ClaimsPrincipal(
                new System.Security.Claims.ClaimsIdentity(new[] { new System.Security.Claims.Claim("name", "testuser") }));

            var validationException = new ValidationException(new[] { "Name is required" });

            _nextMock.Setup(x => x(It.IsAny<HttpContext>()))
                .ThrowsAsync(validationException);

            // Act
            await _middleware.InvokeAsync(httpContext);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Validation exception occurred")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        #endregion

        #region Business Exception Handling

        [Fact]
        public async Task InvokeAsync_MaterialNameAlreadyExistsException_ShouldReturnBadRequest()
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            httpContext.Response.Body = new MemoryStream();
            var exception = new MaterialNameAlreadyExistsException("Duplicate Material");

            _nextMock.Setup(x => x(It.IsAny<HttpContext>()))
                .ThrowsAsync(exception);

            // Act
            await _middleware.InvokeAsync(httpContext);

            // Assert
            httpContext.Response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);

            var responseBody = await ReadResponseBody(httpContext.Response);
            var problemDetails = JsonSerializer.Deserialize<ProblemDetails>(responseBody);

            problemDetails.Title.Should().Be("Business Rule Violation");
            problemDetails.Status.Should().Be(400);
            problemDetails.Detail.Should().Be("A material with the name 'Duplicate Material' already exists.");
            problemDetails.Extensions["materialName"].Should().Be("Duplicate Material");
            problemDetails.Extensions["errorCode"].Should().Be("MATERIAL_NAME_EXISTS");
        }

        [Fact]
        public async Task InvokeAsync_InvalidStockQuantityException_ShouldReturnBadRequest()
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            httpContext.Response.Body = new MemoryStream();
            var exception = new InvalidStockQuantityException(-5);

            _nextMock.Setup(x => x(It.IsAny<HttpContext>()))
                .ThrowsAsync(exception);

            // Act
            await _middleware.InvokeAsync(httpContext);

            // Assert
            httpContext.Response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);

            var responseBody = await ReadResponseBody(httpContext.Response);
            var problemDetails = JsonSerializer.Deserialize<ProblemDetails>(responseBody);

            problemDetails.Title.Should().Be("Business Rule Violation");
            problemDetails.Status.Should().Be(400);
            problemDetails.Extensions["stockQuantity"].Should().Be(-5m);
            problemDetails.Extensions["errorCode"].Should().Be("INVALID_STOCK_QUANTITY");
        }

        [Fact]
        public async Task InvokeAsync_MaterialNotFoundException_ShouldReturnBadRequest()
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            httpContext.Response.Body = new MemoryStream();
            var materialId = Guid.NewGuid();
            var exception = new MaterialNotFoundException(materialId);

            _nextMock.Setup(x => x(It.IsAny<HttpContext>()))
                .ThrowsAsync(exception);

            // Act
            await _middleware.InvokeAsync(httpContext);

            // Assert
            httpContext.Response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);

            var responseBody = await ReadResponseBody(httpContext.Response);
            var problemDetails = JsonSerializer.Deserialize<ProblemDetails>(responseBody);

            problemDetails.Title.Should().Be("Business Rule Violation");
            problemDetails.Status.Should().Be(400);
            problemDetails.Extensions["materialId"].Should().Be(materialId);
            problemDetails.Extensions["errorCode"].Should().Be("MATERIAL_NOT_FOUND");
        }

        #endregion

        #region Authorization Exception Handling

        [Fact]
        public async Task InvokeAsync_UnauthorizedAccessException_ShouldReturnForbidden()
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            httpContext.Response.Body = new MemoryStream();
            var exception = new UnauthorizedAccessException("Access denied");

            _nextMock.Setup(x => x(It.IsAny<HttpContext>()))
                .ThrowsAsync(exception);

            // Act
            await _middleware.InvokeAsync(httpContext);

            // Assert
            httpContext.Response.StatusCode.Should().Be((int)HttpStatusCode.Forbidden);

            var responseBody = await ReadResponseBody(httpContext.Response);
            var problemDetails = JsonSerializer.Deserialize<ProblemDetails>(responseBody);

            problemDetails.Title.Should().Be("Access Denied");
            problemDetails.Status.Should().Be(403);
            problemDetails.Detail.Should().Be("You do not have permission to perform this action.");
            problemDetails.Extensions["errorCode"].Should().Be("ACCESS_DENIED");
        }

        #endregion

        #region Generic Exception Handling

        [Fact]
        public async Task InvokeAsync_GenericException_ShouldReturnInternalServerError()
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            httpContext.Response.Body = new MemoryStream();
            var exception = new Exception("Something went wrong");

            _nextMock.Setup(x => x(It.IsAny<HttpContext>()))
                .ThrowsAsync(exception);

            // Act
            await _middleware.InvokeAsync(httpContext);

            // Assert
            httpContext.Response.StatusCode.Should().Be((int)HttpStatusCode.InternalServerError);

            var responseBody = await ReadResponseBody(httpContext.Response);
            var problemDetails = JsonSerializer.Deserialize<ProblemDetails>(responseBody);

            problemDetails.Title.Should().Be("Internal Server Error");
            problemDetails.Status.Should().Be(500);
            problemDetails.Detail.Should().Be("An unexpected error occurred while processing your request.");
            problemDetails.Extensions["errorCode"].Should().Be("INTERNAL_SERVER_ERROR");
        }

        [Fact]
        public async Task InvokeAsync_GenericException_ShouldLogError()
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Method = "GET";
            httpContext.Request.Path = "/api/materials";
            httpContext.User = new System.Security.Claims.ClaimsPrincipal(
                new System.Security.Claims.ClaimsIdentity(new[] { new System.Security.Claims.Claim("name", "testuser") }));

            var exception = new Exception("Database connection failed");

            _nextMock.Setup(x => x(It.IsAny<HttpContext>()))
                .ThrowsAsync(exception);

            // Act
            await _middleware.InvokeAsync(httpContext);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Unhandled exception occurred")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        #endregion

        #region Response Format Tests

        [Fact]
        public async Task InvokeAsync_AllExceptions_ShouldIncludeInstanceAndExtensions()
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Path = "/api/test";
            httpContext.Response.Body = new MemoryStream();
            var exception = new ValidationException(new[] { "Test error" });

            _nextMock.Setup(x => x(It.IsAny<HttpContext>()))
                .ThrowsAsync(exception);

            // Act
            await _middleware.InvokeAsync(httpContext);

            // Assert
            var responseBody = await ReadResponseBody(httpContext.Response);
            var problemDetails = JsonSerializer.Deserialize<ProblemDetails>(responseBody);

            problemDetails.Instance.Should().Be("/api/test");
            problemDetails.Extensions.Should().NotBeNull();
            problemDetails.Extensions.Should().ContainKey("errorCode");
        }

        [Fact]
        public async Task InvokeAsync_Response_ShouldUseCamelCasePropertyNames()
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            httpContext.Response.Body = new MemoryStream();
            var exception = new ValidationException(new[] { "Test error" });

            _nextMock.Setup(x => x(It.IsAny<HttpContext>()))
                .ThrowsAsync(exception);

            // Act
            await _middleware.InvokeAsync(httpContext);

            // Assert
            var responseBody = await ReadResponseBody(httpContext.Response);
            responseBody.Should().Contain("errorCode"); // camelCase
            responseBody.Should().Contain("status");
            responseBody.Should().Contain("title");
            responseBody.Should().Contain("detail");
        }

        #endregion

        #region User Context Tests

        [Fact]
        public async Task InvokeAsync_WithAuthenticatedUser_ShouldIncludeUserInLogs()
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Method = "POST";
            httpContext.Request.Path = "/api/materials";
            httpContext.User = new System.Security.Claims.ClaimsPrincipal(
                new System.Security.Claims.ClaimsIdentity(new[] { new System.Security.Claims.Claim("name", "john.doe") }));

            var exception = new ValidationException(new[] { "Name required" });

            _nextMock.Setup(x => x(It.IsAny<HttpContext>()))
                .ThrowsAsync(exception);

            // Act
            await _middleware.InvokeAsync(httpContext);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("john.doe")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task InvokeAsync_WithoutUser_ShouldUseAnonymousInLogs()
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Method = "POST";
            httpContext.Request.Path = "/api/materials";
            // No user set

            var exception = new ValidationException(new[] { "Name required" });

            _nextMock.Setup(x => x(It.IsAny<HttpContext>()))
                .ThrowsAsync(exception);

            // Act
            await _middleware.InvokeAsync(httpContext);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Anonymous")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        #endregion

        #region Helper Methods

        private async Task<string> ReadResponseBody(HttpResponse response)
        {
            response.Body.Position = 0;
            using var reader = new StreamReader(response.Body);
            return await reader.ReadToEndAsync();
        }

        #endregion
    }

    // ProblemDetails class for testing (simplified version)
    public class ProblemDetails
    {
        public string? Type { get; set; }
        public string? Title { get; set; }
        public int Status { get; set; }
        public string? Detail { get; set; }
        public string? Instance { get; set; }
        public Dictionary<string, object?> Extensions { get; set; } = new();
    }
}