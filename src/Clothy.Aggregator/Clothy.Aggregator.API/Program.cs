using Clothy.Aggregator.Aggregate.Clients;
using Clothy.Aggregator.Aggregate.Clients.Interfaces;
using Clothy.Aggregator.Aggregate.Services;
using Clothy.Aggregator.Aggregate.Services.Interfaces;
using Clothy.Aggregator.API.Middleware;
using Clothy.ServiceDefaults.Middleware.CorrelationId;
using Clothy.ServiceDefaults.Middleware.Grpc;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.DefaultIgnoreCondition =
            System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddServiceDiscovery();

string? redisConnStr = builder.Configuration.GetConnectionString("clothy-redis");
if (!string.IsNullOrEmpty(redisConnStr))
    builder.Services.AddStackExchangeRedisCache(options =>
        options.Configuration = redisConnStr);
else builder.AddRedisClient("clothy-redis");

builder.Services.AddMemoryCache(options =>
{
    options.SizeLimit = 1024;

    options.CompactionPercentage = 0.2;
});

builder.Services.AddScoped<IFilterGrpcClient, FilterGrpcClient>();
builder.Services.AddScoped<IClotheGrpcClient, ClotheGrpcClient>();
builder.Services.AddScoped<IReviewGrpcClient, ReviewGrpcClient>();
builder.Services.AddScoped<IStockGrpcClient, StockGrpcClient>();
builder.Services.AddScoped<IOrderGrpcClient, OrderGrpcClient>();

builder.Services.AddScoped<IClotheAggregateService, ClotheAggregateService>();
builder.Services.AddScoped<IDashboardAggregateService, DashboardAggregateService>();

builder.Services.AddConfiguredGrpcClient<ClotheFilterServiceGrpc.ClotheFilterServiceGrpcClient>("catalog")
    .AddStandardResilienceHandler(resilience =>
    {
        resilience.Retry.MaxRetryAttempts = 2;
        resilience.CircuitBreaker.FailureRatio = 0.6;
    });

builder.Services.AddConfiguredGrpcClient<ClotheStockService.ClotheStockServiceClient>("catalog")
    .AddStandardResilienceHandler(resilience =>
    {
        resilience.Retry.MaxRetryAttempts = 2;
        resilience.CircuitBreaker.FailureRatio = 0.6;
    });

builder.Services.AddConfiguredGrpcClient<ClotheServiceGrpc.ClotheServiceGrpcClient>("catalog")
    .AddStandardResilienceHandler(resilience =>
    {
        resilience.Retry.MaxRetryAttempts = 2;
        resilience.CircuitBreaker.FailureRatio = 0.4;
    }); 

builder.Services.AddConfiguredGrpcClient<ReviewServiceGrpc.ReviewServiceGrpcClient>("reviews")
    .AddStandardResilienceHandler(resilience =>
    {
        resilience.Retry.MaxRetryAttempts = 3;
        resilience.CircuitBreaker.FailureRatio = 0.6;
    });

builder.Services.AddConfiguredGrpcClient<OrderStatsService.OrderStatsServiceClient>("orders")
    .AddStandardResilienceHandler(resilience =>
    {
        resilience.Retry.MaxRetryAttempts = 3;
        resilience.CircuitBreaker.FailureRatio = 0.6;
    });

builder.Services.AddTransient<CorrelationIdDelegatingHandler>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseServiceDefaults();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

await app.RunAsync();