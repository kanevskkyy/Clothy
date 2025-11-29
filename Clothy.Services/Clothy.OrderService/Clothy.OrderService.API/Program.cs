using System.Reflection;
using Clothy.OrderService.API.Middleware;
using Clothy.OrderService.BLL.FluentValidation.OrderStatusValidation;
using Clothy.OrderService.BLL.Interfaces;
using Clothy.OrderService.BLL.Mapper;
using Clothy.OrderService.BLL.RedisCache.CityCache;
using Clothy.OrderService.BLL.RedisCache.DeliveryProviderCache;
using Clothy.OrderService.BLL.RedisCache.OrdersCache;
using Clothy.OrderService.BLL.RedisCache.OrderStatusCache;
using Clothy.OrderService.BLL.Services;
using Clothy.OrderService.DAL.ConnectionFactory;
using Clothy.OrderService.DAL.Interfaces;
using Clothy.OrderService.DAL.Repositories;
using Clothy.OrderService.DAL.UOW;
using Clothy.OrderService.Domain.Entities;
using Clothy.Shared.Cache.Interfaces;
using Clothy.Shared.Helpers;
using FluentValidation;
using Clothy.OrderService.gRPC.Client.Services.Interfaces;
using Clothy.OrderService.gRPC.Client.Services;
using FluentValidation.AspNetCore;
using Clothy.OrderService.BLL.RedisCache.RegionCache;
using Clothy.OrderService.BLL.RedisCache.PickupPointsCache;
using Clothy.OrderService.BLL.RedisCache.SettlementCache;
using System.Text.Json;
using System.Diagnostics.Metrics;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using MassTransit;
using Clothy.OrderService.BLL.Consumers;
using Clothy.Shared.Events.OrderEvents;
using Clothy.Shared.Events;
using Clothy.OrderService.DAL.EventLog;
using Clothy.ServiceDefaults.Middleware.Grpc;
using Clothy.ServiceDefaults.Middleware.OpenTelemetry;
using Clothy.ServiceDefaults.Middleware.Redis;

var builder = WebApplication.CreateBuilder(args);

//HEALTH CHECK FOR POSTGRES
builder.Services.AddHealthChecks()
    .AddNpgSql(
        connectionString: builder.Configuration.GetConnectionString("ClothyOrder")!,
        name: "order-postgres-db",
        healthQuery: "SELECT 1;",
        failureStatus: HealthStatus.Unhealthy,
        tags: new[] { "ready", "db", "sql", "postgres" }
    )
    .AddRedis(
        redisConnectionString: builder.Configuration.GetConnectionString("clothy-redis")!,
        name: "redis-cache",
        failureStatus: HealthStatus.Degraded,
        tags: new[] { "ready", "cache", "redis" },
        timeout: TimeSpan.FromSeconds(3)
    );
//
builder.AddServiceDefaults();

builder.Services.AddSingleton<IConnectionFactory>(sp =>
{
    string? connectionString = builder.Configuration.GetConnectionString("ClothyOrder");
    return new ConnectionFactory(connectionString);
});

builder.AddRedisClient("clothy-redis");

// REGISTER REPOSITORY
builder.Services.AddScoped<IOrderStatusRepository, OrderStatusRepository>();
builder.Services.AddScoped<IDeliveryProviderRepository, DeliveryProviderRepository>();
builder.Services.AddScoped<ICityRepository, CityRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IOrderItemRepository, OrderItemRepository>();
builder.Services.AddScoped<IDeliveryDetailRepository, DeliveryDetailRepository>();
builder.Services.AddScoped<IRegionRepository, RegionRepository>();
builder.Services.AddScoped<ISettlementRepository, SettlementRepository>();
builder.Services.AddScoped<IPickupPointRepository, PickupPointRepository>();

// REGISTER UNIT OF WORK
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// AUTO MAPPER REGISTER
builder.Services.AddAutoMapper(typeof(CityProfile).Assembly);

// SERVICES REGISTER
builder.Services.AddScoped<ICityService, CityService>();
builder.Services.AddScoped<IDeliveryProviderService, DeliveryProviderService>();
builder.Services.AddScoped<IOrderStatusService, OrderStatusService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IRegionService, RegionService>();
builder.Services.AddScoped<ISettlementService, SettlementService>();
builder.Services.AddScoped<IPickupPointService, PickupPointService>();
builder.Services.AddScoped<IOrderItemService, OrderItemService>();

//RabbitMQ
builder.Services.AddScoped<IEventLogService, EventLogService>();

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<DeleteOrderItemConsumerService>();
    x.AddConsumer<UpdateOrderItemConsumerService>();
    x.AddConsumer<UserUpdatedConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(builder.Configuration.GetConnectionString("rabbitmq"));
        cfg.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(5)));

        cfg.ReceiveEndpoint("order-service-clothe-updated-queue", e =>
        {
            e.ConfigureConsumer<UpdateOrderItemConsumerService>(context);
            e.Bind("clothe-item-updated");
        });

        cfg.ReceiveEndpoint("order-service-clothe-deleted-queue", e =>
        {
            e.ConfigureConsumer<DeleteOrderItemConsumerService>(context);
            e.Bind("clothe-item-deleted");
        });

        cfg.ReceiveEndpoint("order-service-user-consumer-queue", e =>
        {
            e.ConfigureConsumer<UserUpdatedConsumer>(context);
            e.Bind("user-updated");
        });

        cfg.Message<OrderCreatedEvent>(e => e.SetEntityName("order-created"));
    });
});
//

// CLOUDINARY CONFIG
builder.Services.AddCloudinary(builder.Configuration);
//

//REDIS
builder.Services.AddTransient<ICachePreloader, OrderCachePreloader>();
builder.Services.AddTransient<IEntityCacheInvalidationService<Order>, OrderCacheInvalidationService>();

builder.Services.AddTransient<ICachePreloader, OrderStatusCachePreloader>();
builder.Services.AddTransient<IEntityCacheInvalidationService<OrderStatus>, OrderStatusCacheInvalidationService>();

builder.Services.AddTransient<ICachePreloader, DeliveryProviderCachePreloader>();
builder.Services.AddTransient<IEntityCacheInvalidationService<DeliveryProvider>, DeliveryProviderCacheInvalidationService>();

builder.Services.AddTransient<ICachePreloader, CityCachePreloader>();
builder.Services.AddTransient<IEntityCacheInvalidationService<City>, CityCacheInvalidationService>();

builder.Services.AddTransient<ICachePreloader, RegionCachePreloader>();
builder.Services.AddTransient<IEntityCacheInvalidationService<Region>, RegionCacheInvalidationService>();

builder.Services.AddTransient<ICachePreloader, PickupPointCachePreloader>();
builder.Services.AddTransient<IEntityCacheInvalidationService<PickupPoints>, PickupPointCacheInvalidationService>();

builder.Services.AddTransient<ICachePreloader, SettlementCachePreloader>();
builder.Services.AddTransient<IEntityCacheInvalidationService<Settlement>, SettlementCacheInvalidationService>();
//

// FLUENT VALIDATION
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssembly(typeof(OrderStatusCreateDTOValidator).Assembly);

builder.Services.AddEndpointsApiExplorer();

//GRPC 
builder.Services.AddScoped<IOrderItemValidatorGrpcClient, OrderItemValidatorGrpcClient>();
builder.Services.AddScoped<IBasketGrpcClient, BasketGrpcClient>();

builder.Services.AddConfiguredGrpcClient<OrderItemValidator.OrderItemValidatorClient>("catalog")
    .AddStandardResilienceHandler(resilience =>
    {
        resilience.Retry.MaxRetryAttempts = 3;
        resilience.CircuitBreaker.FailureRatio = 0.3;
    });

builder.Services.AddConfiguredGrpcClient<BasketGrpc.BasketGrpcClient>("basket")
    .AddStandardResilienceHandler(resilience =>
    {
        resilience.Retry.MaxRetryAttempts = 3;
        resilience.CircuitBreaker.FailureRatio = 0.3;
    });

// OPEN TELEMETRY CONFIG
builder.Services.AddConfiguredOpenTelemetry("OrderService", builder.Configuration);
Meter meter = builder.Services.AddOrGetMeter("OrderService");
builder.Services.AddSingleton(meter);
//

var app = builder.Build();

await app.PreloadCachesAsync();

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseServiceDefaults();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();