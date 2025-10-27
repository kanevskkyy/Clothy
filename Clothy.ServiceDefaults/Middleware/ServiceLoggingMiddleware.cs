using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Serilog;

namespace Clothy.ServiceDefaults.Middleware
{
    public class ServiceLoggingMiddleware
    {
        private RequestDelegate next;
        private const string CORRELATION_HEADER = "X-Correlation-Id";

        public ServiceLoggingMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var sw = Stopwatch.StartNew();
            string correlationId = context.Items[CORRELATION_HEADER]?.ToString() ?? "unknown";
            string clientIp = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            if (clientIp == "::1") clientIp = "127.0.0.1";

            Log.Information(
                "Request started | {Method} {Path} | CorrelationId={CorrelationId} | ClientIp={ClientIp}",
                context.Request.Method,
                context.Request.Path,
                correlationId,
                clientIp);

            try
            {
                await next(context);
                sw.Stop();

                var responseSize = context.Response.ContentLength ?? 0;

                Log.Information(
                    "Request finished | {Method} {Path} | Status={StatusCode} | DurationMs={ElapsedMs} | ResponseSize={ResponseSize}B | CorrelationId={CorrelationId}",
                    context.Request.Method,
                    context.Request.Path,
                    context.Response.StatusCode,
                    sw.ElapsedMilliseconds,
                    responseSize,
                    correlationId);

                if (sw.ElapsedMilliseconds > 2000)
                {
                    Log.Warning(
                        "Slow request | {Method} {Path} | DurationMs={ElapsedMs} | CorrelationId={CorrelationId}",
                        context.Request.Method,
                        context.Request.Path,
                        sw.ElapsedMilliseconds,
                        correlationId);
                }
            }
            catch (Exception ex)
            {
                sw.Stop();
                Log.Error(ex,
                    "Request error | {Method} {Path} | DurationMs={ElapsedMs} | CorrelationId={CorrelationId}",
                    context.Request.Method,
                    context.Request.Path,
                    sw.ElapsedMilliseconds,
                    correlationId);
                throw;
            }
        }
    }
}
