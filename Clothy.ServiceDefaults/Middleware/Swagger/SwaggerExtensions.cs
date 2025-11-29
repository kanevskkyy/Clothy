using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Clothy.ServiceDefaults.Middleware
{
    public static class SwaggerExtensions
    {
        public static IServiceCollection AddSwaggerWithXmlComments(this IServiceCollection services)
        {
            services.AddSwaggerGen(options =>
            {
                Assembly? entryAssembly = Assembly.GetEntryAssembly();
                if (entryAssembly != null)
                {
                    string xmlFilename = $"{entryAssembly.GetName().Name}.xml";
                    string xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFilename);

                    if (File.Exists(xmlPath))
                    {
                        options.IncludeXmlComments(xmlPath);
                    }
                }
            });

            return services;
        }
    }
}
