using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;
using FunnyActivities.Domain.Exceptions;

namespace FunnyActivities.CrossCuttingConcerns.ErrorHandling;

/// <summary>
/// Global exception handling middleware that catches and handles various types of exceptions,
/// returning appropriate HTTP responses with ProblemDetails format.
/// </summary>
public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ExceptionMiddleware"/> class.
    /// </summary>
    /// <param name="next">The next middleware in the pipeline.</param>
    /// <param name="logger">The logger instance for logging exceptions.</param>
    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    /// <summary>
    /// Invokes the middleware to handle the HTTP request and catch exceptions.
    /// </summary>
    /// <param name="context">The HTTP context for the current request.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    /// <summary>
    /// Handles exceptions by determining the appropriate HTTP status code and logging level,
    /// then returns a ProblemDetails response.
    /// </summary>
    /// <param name="context">The HTTP context for the current request.</param>
    /// <param name="exception">The exception that was caught.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var problemDetails = new ProblemDetails
        {
            Instance = context.Request.Path,
            Extensions = new Dictionary<string, object?>()
        };

        string user = context.User?.Identity?.Name ?? "Anonymous";
        string method = context.Request.Method;
        string path = context.Request.Path;

        switch (exception)
        {
            case ValidationException validationEx:
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                problemDetails.Title = "Validation Error";
                problemDetails.Detail = "One or more validation errors occurred.";
                problemDetails.Status = 400;
                problemDetails.Extensions["errors"] = validationEx.Errors;
                problemDetails.Extensions["errorCode"] = "VALIDATION_FAILED";

                _logger.LogWarning(exception,
                    "Validation exception occurred. Method: {Method}, Path: {Path}, User: {User}, Errors: {Errors}",
                    method, path, user, string.Join("; ", validationEx.Errors));
                break;

            case MaterialNameAlreadyExistsException materialEx:
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                problemDetails.Title = "Business Rule Violation";
                problemDetails.Detail = exception.Message;
                problemDetails.Status = 400;
                problemDetails.Extensions["materialName"] = materialEx.MaterialName;
                problemDetails.Extensions["errorCode"] = "MATERIAL_NAME_EXISTS";

                _logger.LogWarning(exception,
                    "Business exception occurred. Method: {Method}, Path: {Path}, User: {User}, MaterialName: {MaterialName}",
                    method, path, user, materialEx.MaterialName);
                break;

            case InvalidStockQuantityException stockEx:
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                problemDetails.Title = "Business Rule Violation";
                problemDetails.Detail = exception.Message;
                problemDetails.Status = 400;
                problemDetails.Extensions["stockQuantity"] = stockEx.StockQuantity;
                problemDetails.Extensions["errorCode"] = "INVALID_STOCK_QUANTITY";

                _logger.LogWarning(exception,
                    "Business exception occurred. Method: {Method}, Path: {Path}, User: {User}, StockQuantity: {StockQuantity}",
                    method, path, user, stockEx.StockQuantity);
                break;

            case MaterialNotFoundException notFoundEx:
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                problemDetails.Title = "Business Rule Violation";
                problemDetails.Detail = exception.Message;
                problemDetails.Status = 400;
                problemDetails.Extensions["materialId"] = notFoundEx.MaterialId;
                problemDetails.Extensions["errorCode"] = "MATERIAL_NOT_FOUND";

                _logger.LogWarning(exception,
                    "Business exception occurred. Method: {Method}, Path: {Path}, User: {User}, MaterialId: {MaterialId}",
                    method, path, user, notFoundEx.MaterialId);
                break;

            case MaterialCannotBeDeletedException deleteEx:
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                problemDetails.Title = "Business Rule Violation";
                problemDetails.Detail = exception.Message;
                problemDetails.Status = 400;
                problemDetails.Extensions["materialId"] = deleteEx.MaterialId;
                problemDetails.Extensions["errorCode"] = "MATERIAL_CANNOT_BE_DELETED";

                _logger.LogWarning(exception,
                    "Business exception occurred. Method: {Method}, Path: {Path}, User: {User}, MaterialId: {MaterialId}",
                    method, path, user, deleteEx.MaterialId);
                break;

            case MaterialPhotoNotFoundException photoEx:
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                problemDetails.Title = "Business Rule Violation";
                problemDetails.Detail = exception.Message;
                problemDetails.Status = 400;
                problemDetails.Extensions["materialId"] = photoEx.MaterialId;
                problemDetails.Extensions["photoUrl"] = photoEx.PhotoUrl;
                problemDetails.Extensions["errorCode"] = "MATERIAL_PHOTO_NOT_FOUND";

                _logger.LogWarning(exception,
                    "Business exception occurred. Method: {Method}, Path: {Path}, User: {User}, MaterialId: {MaterialId}, PhotoUrl: {PhotoUrl}",
                    method, path, user, photoEx.MaterialId, photoEx.PhotoUrl);
                break;

            case BaseProductHasVariantsException baseProductEx:
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                problemDetails.Title = "Business Rule Violation";
                problemDetails.Detail = exception.Message;
                problemDetails.Status = 400;
                problemDetails.Extensions["errorCode"] = "BASE_PRODUCT_HAS_VARIANTS";

                _logger.LogWarning(exception,
                    "Middleware caught BaseProductHasVariantsException. Method: {Method}, Path: {Path}, User: {User}",
                    method, path, user);
                break;

            case UnauthorizedAccessException authEx:
                context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                problemDetails.Title = "Access Denied";
                problemDetails.Detail = "You do not have permission to perform this action.";
                problemDetails.Status = 403;
                problemDetails.Extensions["errorCode"] = "ACCESS_DENIED";

                _logger.LogWarning(exception,
                    "Authorization exception occurred. Method: {Method}, Path: {Path}, User: {User}",
                    method, path, user);
                break;

            default:
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                problemDetails.Title = "Internal Server Error";
                problemDetails.Detail = "An unexpected error occurred while processing your request.";
                problemDetails.Status = 500;
                problemDetails.Extensions["errorCode"] = "INTERNAL_SERVER_ERROR";

                _logger.LogError(exception,
                    "Unhandled exception occurred. Method: {Method}, Path: {Path}, User: {User}",
                    method, path, user);
                break;
        }

        context.Response.ContentType = "application/problem+json";

        var json = JsonSerializer.Serialize(problemDetails, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        });

        await context.Response.WriteAsync(json);
    }
}