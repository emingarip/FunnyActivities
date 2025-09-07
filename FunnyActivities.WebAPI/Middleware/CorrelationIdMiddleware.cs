using Microsoft.AspNetCore.Http;
using Serilog.Context;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace FunnyActivities.WebAPI.Middleware
{
    public class CorrelationIdMiddleware
    {
        private readonly RequestDelegate _next;

        public CorrelationIdMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            string correlationId = context.Request.Headers["X-Correlation-ID"].FirstOrDefault();

            if (string.IsNullOrEmpty(correlationId))
            {
                correlationId = Guid.NewGuid().ToString();
            }

            context.Response.Headers.Add("X-Correlation-ID", correlationId);

            using (LogContext.PushProperty("CorrelationId", correlationId))
            {
                await _next(context);
            }
        }
    }
}