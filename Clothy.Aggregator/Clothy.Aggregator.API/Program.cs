using Clothy.Aggregator.Aggregate.Clients;
using Clothy.Aggregator.Aggregate.Clients.Interfaces;
using Clothy.Aggregator.Aggregate.Services;
using Clothy.Aggregator.Aggregate.Services.Interfaces;
using Clothy.ServiceDefaults.Middleware;
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

builder.AddRedisClient("clothy-redis");
builder.Services.AddMemoryCache(options =>
{
    options.SizeLimit = 1024;

    options.CompactionPercentage = 0.2;
});

// REGISTER CLIENTS
builder.Services.AddScoped<IFilterGrpcClient, FilterGrpcClient>();
builder.Services.AddScoped<IClotheGrpcClient, ClotheGrpcClient>();
builder.Services.AddScoped<IReviewGrpcClient, ReviewGrpcClient>();
//

// REGISTER SERVICES
builder.Services.AddScoped<IClotheAggregateService, ClotheAggregateService>();
//

builder.Services.AddConfiguredGrpcClient<ClotheFilterServiceGrpc.ClotheFilterServiceGrpcClient>("catalog");
builder.Services.AddConfiguredGrpcClient<ClotheServiceGrpc.ClotheServiceGrpcClient>("catalog");
builder.Services.AddConfiguredGrpcClient<ReviewServiceGrpc.ReviewServiceGrpcClient>("reviews");

//builder.Services.AddHttpContextAccessor();
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

app.UseCorrelationId();
app.MapControllers();

await app.RunAsync();