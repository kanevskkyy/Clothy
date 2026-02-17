using Clothy.ServiceDefaults;
using Clothy.ServiceDefaults.Middleware.Routes;
using Yarp.ReverseProxy.Transforms;

var builder = WebApplication.CreateBuilder(args);

builder.AddGatewayDefaults();

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
    .AddTransforms(transformBuilderContext =>
    {
        transformBuilderContext.AddRequestTransform(async reqContext =>
        {
            if (reqContext.HttpContext.Request.Headers.TryGetValue("X-Correlation-Id", out var correlationId))
            {
                reqContext.ProxyRequest.Headers.Add("X-Correlation-Id", correlationId.ToString());
            }
        });
    });

var app = builder.Build();

app.UseGatewayDefaults();

app.UseMiddleware<RouteMetadataMiddleware>();
app.MapDefaultEndpoints();
app.MapReverseProxy();

await app.RunAsync();