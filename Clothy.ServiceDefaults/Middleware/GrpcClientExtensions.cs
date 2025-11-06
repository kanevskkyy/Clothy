using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Clothy.ServiceDefaults.Middleware
{
    public static class GrpcClientExtensions
    {
        public static IHttpClientBuilder AddConfiguredGrpcClient<TClient>(this IServiceCollection services, string serviceName) where TClient : class
        {
            return services
                .AddGrpcClient<TClient>(options =>
                {
                    options.Address = new Uri($"https://{serviceName}");
                })
                .ConfigureChannel(channelOptions =>
                {
                    channelOptions.MaxReceiveMessageSize = 5 * 1024 * 1024;
                    channelOptions.MaxSendMessageSize = 5 * 1024 * 1024;
                })
                .AddServiceDiscovery();
        }
    }
}
