using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.Shared.Cache.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Clothy.ServiceDefaults.Middleware
{
    public static class ApplicationExtensions
    {
        public static async Task PreloadCachesAsync(this WebApplication app, CancellationToken cancellationToken = default)
        {
            using var scope = app.Services.CreateScope();
            var preloaders = scope.ServiceProvider.GetServices<ICachePreloader>();

            foreach (var preloader in preloaders)
            {
                await preloader.PreloadAsync(cancellationToken);
            }
        }
    }
}
