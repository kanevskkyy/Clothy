using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Clothy.Shared.Helpers.CloudinaryConfig.ImageService;

namespace Clothy.Shared.Helpers.CloudinaryConfig
{
    public static class CloudinaryExtensions
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

            services.AddScoped<IImageService, CloudinaryService>();

            return services;
        }
    }
}