using System;
using System.Collections.Concurrent;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using System.Threading.Tasks;

namespace Clothy.ServiceDefaults.Middleware
{
    public class RouteMetadataMiddleware
    {
        private readonly RequestDelegate _next;
        private const long MAX_REQUEST_SIZE = 10 * 1024 * 1024;
        private static readonly MemoryCache _rateLimitCache = new(new MemoryCacheOptions());

        public RouteMetadataMiddleware(RequestDelegate next)
        {
            _next = next;
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
                            string clientKey = GetClientKey(context);
                            int counter = _rateLimitCache.GetOrCreate(clientKey, entry =>
                            {
                                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1);
                                return 0;
                            });

                            if (counter >= limit)
                            {
                                context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                                await context.Response.WriteAsync("Rate limit exceeded. Try again later.");
                                return;
                            }
                            else
                            {
                                _rateLimitCache.Set(clientKey, counter + 1, TimeSpan.FromMinutes(1));
                                context.Items["RouteRateLimit"] = limit;
                            }
                        }
                    }
                }
            }

            await _next(context);
        }

        private string GetClientKey(HttpContext context)
        {
            return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        }
    }
}