using Clothy.ServiceDefaults.Middleware;
using Yarp.ReverseProxy.Transforms;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.ConfigureHttpClientDefaults(http =>
{
    http.AddStandardResilienceHandler();
    http.AddServiceDiscovery();
});

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
    })
    .ConfigureHttpClient((context, httpClient) =>
    {
        httpClient.ConnectTimeout = TimeSpan.FromSeconds(10); 
    });



builder.Services.AddHttpContextAccessor();

var app = builder.Build();

app.UseServiceDefaults();
app.UseMiddleware<RouteMetadataMiddleware>();

app.MapDefaultEndpoints();
app.MapReverseProxy();

await app.RunAsync();