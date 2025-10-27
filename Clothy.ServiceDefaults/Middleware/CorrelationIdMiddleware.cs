using System;
using Microsoft.AspNetCore.Http;
using Serilog.Context;

namespace Clothy.ServiceDefaults.Middleware
{
    public class CorrelationIdMiddleware
    {
        private RequestDelegate next;
        private const string CORRELATION_ID_HEADER = "X-Correlation-Id";

        public CorrelationIdMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            string correlationId;

            if (!context.Request.Headers.TryGetValue(CORRELATION_ID_HEADER, out var headerValue))
            {
                correlationId = Guid.NewGuid().ToString();
                context.Request.Headers[CORRELATION_ID_HEADER] = correlationId;
            }
            else
            {
                correlationId = headerValue.ToString();
            }

            context.Items[CORRELATION_ID_HEADER] = correlationId;

            context.Response.Headers[CORRELATION_ID_HEADER] = correlationId;

            using (LogContext.PushProperty("CorrelationId", correlationId))
            {
                await next(context);
            }
        }
    }
}