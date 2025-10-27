using System;
using Microsoft.AspNetCore.Http;
using Serilog.Context;

namespace Clothy.ServiceDefaults.Middleware
{
    public class CorrelationIdMiddleware
    {
        private readonly RequestDelegate _next;
        private const string CorrelationIdHeader = "X-Correlation-Id";

        public CorrelationIdMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            string correlationId;

            if (!context.Request.Headers.TryGetValue(CorrelationIdHeader, out var headerValue))
            {
                correlationId = Guid.NewGuid().ToString();
                context.Request.Headers[CorrelationIdHeader] = correlationId;
            }
            else
            {
                correlationId = headerValue.ToString();
            }

            context.Items[CorrelationIdHeader] = correlationId;

            context.Response.Headers[CorrelationIdHeader] = correlationId;

            using (LogContext.PushProperty("CorrelationId", correlationId))
            {
                await _next(context);
            }
        }
    }
}