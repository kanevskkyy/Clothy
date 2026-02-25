using Clothy.OrderService.API.Middleware;
using Clothy.OrderService.BLL.Config;
using Clothy.OrderService.BLL.Consumers;
using Clothy.OrderService.BLL.FluentValidation.OrderValidation;
using Clothy.OrderService.BLL.Interfaces;
using Clothy.OrderService.BLL.Mapper;
using Clothy.OrderService.BLL.RedisCache;
using Clothy.OrderService.BLL.RedisCache.PickupPointsCache;
using Clothy.OrderService.BLL.Services;
using Clothy.OrderService.BLL.Services.BackgroundServices;
using Clothy.OrderService.DAL.ConnectionFactory;
using Clothy.OrderService.DAL.EventLog;
using Clothy.OrderService.DAL.Interfaces;
using Clothy.OrderService.DAL.Repositories;
using Clothy.OrderService.DAL.UOW;
using Clothy.OrderService.Domain.Entities;
using Clothy.OrderService.gRPC.Client.Services;
using Clothy.OrderService.gRPC.Client.Services.Interfaces;
using Clothy.OrderService.gRPC.Server.Services;
using Clothy.ServiceDefaults.Middleware.Grpc;
using Clothy.ServiceDefaults.Middleware.OpenTelemetry;
using Clothy.Shared.Cache.Interfaces;
using Clothy.Shared.Events;
using Clothy.Shared.Events.EmailEvents;
using Clothy.Shared.Events.OrderEvents;
using Clothy.Shared.Helpers.CloudinaryConfig;
using DotNetEnv;
using FluentValidation;
using FluentValidation.AspNetCore;
using MassTransit;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Diagnostics.Metrics;

Env.Load();

var builder = WebApplication.CreateBuilder(args);

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
builder.AddServiceDefaults();

builder.Services.AddSingleton<IConnectionFactory>(sp =>
{
    string? connectionString = builder.Configuration.GetConnectionString("ClothyOrder");
    return new ConnectionFactory(connectionString);
});

builder.AddRedisClient("clothy-redis");

builder.Services.PostConfigure<NovaPoshtaConfig>(options =>
{
    options.APIKey = Environment.GetEnvironmentVariable("NOVAPOSHTA__API_KEY");
});

builder.Services.AddScoped<IDeliveryProviderRepository, DeliveryProviderRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IOrderItemRepository, OrderItemRepository>();
builder.Services.AddScoped<IDeliveryDetailRepository, DeliveryDetailRepository>();
builder.Services.AddScoped<IRegionRepository, RegionRepository>();
builder.Services.AddScoped<ISettlementRepository, SettlementRepository>();
builder.Services.AddScoped<IPickupPointRepository, PickupPointRepository>();
builder.Services.AddScoped<IOrderReservationRepository, OrderReservationRepository>();

builder.Services.AddScoped<IDeliveryAPIClient, NovaPoshtaAPIClient>();
builder.Services.AddHttpClient("NovaPoshtaAPI", client =>
{
    client.Timeout = TimeSpan.FromSeconds(30);
})
.ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
{
    MaxConnectionsPerServer = 20
});

builder.Services.AddHostedService<ExpiredOrdersCleanupService>();
builder.Services.AddHostedService<PickupPointSyncBackgroundService>();

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

builder.Services.AddGrpc();

builder.Services.AddAutoMapper(typeof(OrderProfile).Assembly);

builder.Services.AddScoped<IDeliveryProviderService, DeliveryProviderService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IRegionService, RegionService>();
builder.Services.AddScoped<ISettlementService, SettlementService>();
builder.Services.AddScoped<IPickupPointService, PickupPointService>();
builder.Services.AddScoped<IOrderItemService, OrderItemService>();

builder.Services.AddScoped<IEventLogService, EventLogService>();

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<DeleteOrderItemConsumerService>();
    x.AddConsumer<UpdateOrderItemConsumerService>();
    x.AddConsumer<OrderPaidConsumerService>();

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

        cfg.ReceiveEndpoint("order-service-order-paid-queue", e =>
        {
            e.ConfigureConsumer<OrderPaidConsumerService>(context);
            e.Bind("order-paid");
        });

        cfg.Message<OrderCreatedEvent>(e => e.SetEntityName("order-created"));
        cfg.Message<OrderDeliveredEmailEvent>(e => e.SetEntityName("send-notification-order-delivered"));
        cfg.Message<OrderCreatedEmailEvent>(e => e.SetEntityName("send-notification-order-created"));
        cfg.Message<OrderShippedEmailEvent>(e => e.SetEntityName("send-notification-order-shipped"));
    });
});

builder.Services.AddCloudinary(builder.Configuration);

builder.Services.AddTransient<IEntityCacheInvalidationService<Order>, OrderCacheInvalidationService>();
builder.Services.AddTransient<IEntityCacheInvalidationService<OrderStatus>, OrderStatusCacheInvalidationService>();
builder.Services.AddTransient<IEntityCacheInvalidationService<DeliveryProvider>, DeliveryProviderCacheInvalidationService>();
builder.Services.AddTransient<IEntityCacheInvalidationService<Region>, RegionCacheInvalidationService>();
builder.Services.AddTransient<IEntityCacheInvalidationService<PickupPoints>, PickupPointCacheInvalidationService>();
builder.Services.AddTransient<IEntityCacheInvalidationService<Settlement>, SettlementCacheInvalidationService>();

builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssembly(typeof(OrderCreateDTOValidator).Assembly);

builder.Services.AddEndpointsApiExplorer();

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

builder.Services.AddConfiguredOpenTelemetry("OrderService", builder.Configuration);
Meter meter = builder.Services.AddOrGetMeter("OrderService");
builder.Services.AddSingleton(meter);

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseServiceDefaults();

app.MapGrpcService<CheckUserPurchasedGrpcImpl>();
app.MapGrpcService<GetOrderInfoGrpcServer>();
app.MapGrpcService<OrderStatsServiceImpl>();

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