using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Clothy.ServiceDefaults.Middleware
{
    public class RouteMetadataMiddleware
    {
        private RequestDelegate next;
        private const long MAX_REQUEST_SIZE = 10 * 1024 * 1024;

        public RouteMetadataMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.Request.ContentLength.HasValue && context.Request.ContentLength.Value > MAX_REQUEST_SIZE)
            {
                context.Response.StatusCode = StatusCodes.Status413PayloadTooLarge;
                await context.Response.WriteAsync($"Request payload too large. Max allowed: {MAX_REQUEST_SIZE} bytes.");
                return;
            }

            var endpoint = context.GetEndpoint();
            if (endpoint != null)
            {
                var metadata = endpoint.Metadata.GetMetadata<IDictionary<string, string>>();
                if (metadata != null)
                {
                    if (metadata.TryGetValue("priority", out var priority))
                    {
                        context.Items["RoutePriority"] = priority;

                        switch (priority.ToLower())
                        {
                            case "high":
                                context.Items["RequestTimeout"] = TimeSpan.FromSeconds(5);
                                context.Items["RetryCount"] = 3;
                                break;
                            
                            case "medium":
                                context.Items["RequestTimeout"] = TimeSpan.FromSeconds(10);
                                context.Items["RetryCount"] = 2;
                                break;
                            
                            case "low":
                                context.Items["RequestTimeout"] = TimeSpan.FromSeconds(20);
                                context.Items["RetryCount"] = 1;
                                break;
                        }
                    }

                    if (metadata.TryGetValue("rateLimit", out var rateLimit))
                    {
                        if (int.TryParse(rateLimit, out var limit))
                        {
                            context.Items["RouteRateLimit"] = limit;
                        }
                    }
                }
            }

            await next(context);
        }
    }
}
