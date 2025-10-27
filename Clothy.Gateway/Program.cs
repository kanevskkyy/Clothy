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
    });


builder.Services.AddHttpContextAccessor();

var app = builder.Build();
app.Use(async (context, next) =>
{
    if (!context.Request.Headers.TryGetValue("X-Correlation-ID", out var correlationId))
    {
        correlationId = Guid.NewGuid().ToString();
    }
    context.Request.Headers["X-Correlation-ID"] = correlationId;

    context.Items["CorrelationId"] = correlationId;

    await next();
});

app.UseCorrelationId();

app.MapDefaultEndpoints();
app.MapReverseProxy();

await app.RunAsync();