using System;
using Clothy.Shared.Helpers.CloudinaryConfig;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

namespace Clothy.Shared.Helpers
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddCloudinary(this IServiceCollection services, IConfiguration configuration)
        {
            string? cloudName = configuration["CloudinarySettings:CloudName"];
            string? apiKey = configuration["CloudinarySettings:ApiKey"];
            string? apiSecret = configuration["CloudinarySettings:ApiSecret"];

            services.Configure<CloudinarySettings>(options =>
            {
                options.CloudName = cloudName;
                options.ApiKey = apiKey;
                options.ApiSecret = apiSecret;
            });

            services.AddScoped<IImageService, ImageService>();

            return services;
        }
    }
}