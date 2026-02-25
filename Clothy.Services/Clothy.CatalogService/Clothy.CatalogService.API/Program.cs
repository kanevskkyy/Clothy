using Clothy.Aggregator.Aggregate.RedisCache;
using Clothy.CatalogService.API.Middleware;
using Clothy.CatalogService.BLL.Consumers;
using Clothy.CatalogService.BLL.FluentValidation.BrandValidation;
using Clothy.CatalogService.BLL.Interfaces;
using Clothy.CatalogService.BLL.Mapper;
using Clothy.CatalogService.BLL.RedisCache;
using Clothy.CatalogService.BLL.Services;
using Clothy.CatalogService.DAL.DB;
using Clothy.CatalogService.DAL.EventLog;
using Clothy.CatalogService.DAL.Interfaces;
using Clothy.CatalogService.DAL.Repositories;
using Clothy.CatalogService.DAL.UOW;
using Clothy.CatalogService.Domain.Entities.Catalog;
using Clothy.CatalogService.Domain.Entities.Clothe;
using Clothy.CatalogService.Domain.Entities.Stock;
using Clothy.CatalogService.gRPC.Server.Services;
using Clothy.ServiceDefaults.Middleware.OpenTelemetry;
using Clothy.Shared.Cache.Interfaces;
using Clothy.Shared.Events;
using Clothy.Shared.Events.ClotheItemEvents;
using Clothy.Shared.Events.EmailEvents.ClotheStockUpdated;
using Clothy.Shared.Helpers.CloudinaryConfig;
using FluentValidation;
using FluentValidation.AspNetCore;
using MassTransit;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Diagnostics.Metrics;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHealthChecks()
    .AddDbContextCheck<ClothyCatalogDbContext>(
        name: "catalog-db-check",
        tags: new[] { "ready", "db", "postgres" },
        failureStatus: HealthStatus.Unhealthy)
    .AddRedis(
        redisConnectionString: builder.Configuration.GetConnectionString("clothy-redis"),
        name: "redis",
        failureStatus: HealthStatus.Degraded,
        tags: new[] { "ready", "cache", "redis" },
        timeout: TimeSpan.FromSeconds(3));

builder.AddServiceDefaults();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<IBrandRepository, BrandRepository>();
builder.Services.AddScoped<IClotheItemRepository, ClotheItemRepository>();
builder.Services.AddScoped<IClothesStockRepository, ClothesStockRepository>();
builder.Services.AddScoped<IColorRepository, ColorRepository>();
builder.Services.AddScoped<IMaterialRepository, MaterialRepository>();
builder.Services.AddScoped<ISizeRepository, SizeRepository>();
builder.Services.AddScoped<ITagRepository, TagRepository>();
builder.Services.AddScoped<ICollectionRepository, CollectionRepository>();
builder.Services.AddScoped<IClothingTypeRepository, ClothingTypeRepository>();
builder.Services.AddScoped<IStockNotificationRepository, StockNotificationRepository>();
builder.Services.AddScoped<IClothePopularityRepository, ClothePopularityRepository>();

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

builder.Services.AddAutoMapper(cfg =>
{
    cfg.AddMaps(typeof(TagProfile).Assembly);
});

builder.Services.AddScoped<ITagService, TagService>();
builder.Services.AddScoped<ISizeService, SizeService>();
builder.Services.AddScoped<IMaterialService, MaterialService>();
builder.Services.AddScoped<IColorService, ColorService>();
builder.Services.AddScoped<IClothingTypeService, ClothingTypeService>();
builder.Services.AddScoped<ICollectionService, CollectionService>();
builder.Services.AddScoped<IBrandService, BrandService>();
builder.Services.AddScoped<IClotheService, ClotheService>();
builder.Services.AddScoped<IClothesStockService, ClothesStockService>();
builder.Services.AddScoped<IStockNotificationService, StockNotificationService>();

builder.Services.AddTransient<IEntityCacheInvalidationService<ClotheItem>, ClotheItemCacheInvalidationService>();
builder.Services.AddTransient<IEntityCacheInvalidationService<ClothesStock>, ClothesStockCacheInvalidationService>();
builder.Services.AddScoped<IEntityCacheInvalidationService<Brand>, BrandCacheInvalidationService>();

builder.Services.AddCloudinary(builder.Configuration);

builder.Services.AddScoped<IFilterCacheInvalidationService, FilterCacheInvalidationService>();

builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssembly(typeof(BrandCreateDTOValidator).Assembly);

builder.Services.AddGrpc();

builder.AddNpgsqlDbContext<ClothyCatalogDbContext>("ClothyCatalogDb");
builder.AddRedisClient("clothy-redis");

await using (var scope = builder.Services.BuildServiceProvider().CreateAsyncScope())
{
    ClothyCatalogDbContext dbContext = scope.ServiceProvider.GetRequiredService<ClothyCatalogDbContext>();
    await dbContext.Database.MigrateAsync();
}

builder.Services.AddConfiguredOpenTelemetry("CatalogService", builder.Configuration);
Meter meter = builder.Services.AddOrGetMeter("CatalogService");
builder.Services.AddSingleton(meter);

builder.Services.AddScoped<IEventLogService, EventLogService>();

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<ClotheStockConsumerService>();

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(builder.Configuration.GetConnectionString("rabbitmq"));
        cfg.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(5)));

        cfg.Message<ClotheItemUpdatedEvent>(e => e.SetEntityName("clothe-item-updated"));
        cfg.Message<ClotheItemDeletedEvent>(e => e.SetEntityName("clothe-item-deleted"));
        cfg.Message<ClotheStockUpdatedEvent>(e => e.SetEntityName("clothe-stock-available"));

        cfg.ReceiveEndpoint("catalog-service-order-created-queue", e =>
        {
            e.ConfigureConsumer<ClotheStockConsumerService>(context);
            e.Bind("order-created"); 
        });

        cfg.ConfigureEndpoints(context);
    });
});

var app = builder.Build();

app.MapGrpcService<OrderItemValidatorService>();
app.MapGrpcService<ClotheItemValidatorService>();
app.MapGrpcService<ClotheFilterService>();
app.MapGrpcService<GetClotheByIdGrpcService>();
app.MapGrpcService<ClotheStockServiceImpl>();

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