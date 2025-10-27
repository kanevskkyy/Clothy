using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;

namespace Clothy.ServiceDefaults.Middleware
{
    public static class ServiceLoggingMiddlewareExtensions
    {
        public static IApplicationBuilder UseServiceLogging(this IApplicationBuilder app)
        {
            return app.UseMiddleware<ServiceLoggingMiddleware>();
        }
    }
}
