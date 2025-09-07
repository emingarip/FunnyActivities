using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace FunnyActivities.WebAPI.Middleware
{
    public class RequestResponseLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestResponseLoggingMiddleware> _logger;

        public RequestResponseLoggingMiddleware(RequestDelegate next, ILogger<RequestResponseLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var stopwatch = Stopwatch.StartNew();
            var request = context.Request;
            var response = context.Response;

            // Log request
            var requestBody = await ReadRequestBodyAsync(request);
            _logger.LogInformation("HTTP Request: {Method} {Path} {QueryString}",
                request.Method,
                request.Path,
                request.QueryString,
                new
                {
                    Headers = SanitizeHeaders(request.Headers),
                    Body = ShouldLogBody(request.ContentType) ? requestBody : "[Body not logged]",
                    ContentType = request.ContentType,
                    ContentLength = request.ContentLength
                });

            // Capture response
            var originalBodyStream = response.Body;
            using var responseBody = new MemoryStream();
            response.Body = responseBody;

            try
            {
                await _next(context);

                stopwatch.Stop();

                // Log response
                responseBody.Seek(0, SeekOrigin.Begin);
                var responseBodyText = await new StreamReader(responseBody).ReadToEndAsync();

                _logger.LogInformation("HTTP Response: {StatusCode} {Method} {Path}",
                    response.StatusCode,
                    request.Method,
                    request.Path,
                    new
                    {
                        Headers = SanitizeHeaders(response.Headers),
                        Body = ShouldLogBody(response.ContentType) ? responseBodyText : "[Body not logged]",
                        ContentType = response.ContentType,
                        ContentLength = responseBody.Length,
                        DurationMs = stopwatch.ElapsedMilliseconds
                    });

                // Copy response back to original stream
                responseBody.Seek(0, SeekOrigin.Begin);
                await responseBody.CopyToAsync(originalBodyStream);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "HTTP Request failed: {Method} {Path} after {DurationMs}ms",
                    request.Method, request.Path, stopwatch.ElapsedMilliseconds);
                throw;
            }
            finally
            {
                response.Body = originalBodyStream;
            }
        }

        private async Task<string> ReadRequestBodyAsync(HttpRequest request)
        {
            request.EnableBuffering();
            using var reader = new StreamReader(request.Body, leaveOpen: true);
            var body = await reader.ReadToEndAsync();
            request.Body.Position = 0;
            return body;
        }

        private bool ShouldLogBody(string contentType)
        {
            if (string.IsNullOrEmpty(contentType)) return false;
            return contentType.Contains("application/json") ||
                   contentType.Contains("text/plain") ||
                   contentType.Contains("application/xml");
        }

        private Dictionary<string, string> SanitizeHeaders(IHeaderDictionary headers)
        {
            var sanitized = new Dictionary<string, string>();
            foreach (var header in headers)
            {
                if (IsSensitiveHeader(header.Key))
                {
                    sanitized[header.Key] = "***";
                }
                else
                {
                    sanitized[header.Key] = string.Join(", ", header.Value);
                }
            }
            return sanitized;
        }

        private bool IsSensitiveHeader(string headerName)
        {
            return headerName.ToLowerInvariant() switch
            {
                "authorization" => true,
                "x-api-key" => true,
                "cookie" => true,
                _ => false
            };
        }
    }
}