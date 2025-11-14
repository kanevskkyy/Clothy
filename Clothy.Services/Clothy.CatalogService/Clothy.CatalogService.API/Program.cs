using Clothy.CatalogService.BLL.Interfaces;
using Clothy.CatalogService.BLL.Mapper;
using Clothy.CatalogService.BLL.Services;
using Clothy.CatalogService.DAL.DB;
using Clothy.CatalogService.DAL.Interfaces;
using Clothy.CatalogService.DAL.Repositories;
using Clothy.CatalogService.DAL.UOW;
using FluentValidation.AspNetCore;
using FluentValidation;
using Clothy.CatalogService.BLL.FluentValidation.BrandValidation;
using Clothy.ServiceDefaults.Middleware;
using Clothy.CatalogService.API.Middleware;
using Clothy.Shared.Helpers;
using Clothy.CatalogService.BLL.RedisCache.Clothe;
using Clothy.CatalogService.BLL.RedisCache.StockCache;
using Clothy.CatalogService.Domain.Entities;
using Clothy.Shared.Cache.Interfaces;
using Clothy.CatalogService.gRPC.Server.Services;
using Clothy.Aggregator.Aggregate.RedisCache;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Diagnostics.Metrics;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

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

// REGISTER REPOSITORIES DI
builder.Services.AddScoped<IBrandRepository, BrandRepository>();
builder.Services.AddScoped<IClotheItemRepository, ClotheItemRepository>();
builder.Services.AddScoped<IClothesStockRepository, ClothesStockRepository>();
builder.Services.AddScoped<IColorRepository, ColorRepository>();
builder.Services.AddScoped<IMaterialRepository, MaterialRepository>();
builder.Services.AddScoped<ISizeRepository, SizeRepository>();
builder.Services.AddScoped<ITagRepository, TagRepository>();
builder.Services.AddScoped<ICollectionRepository, CollectionRepository>();
builder.Services.AddScoped<IClothingTypeRepository, ClothingTypeRepository>();

// REGISTER UNIT OF WORK
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// REGISTER AUTO MAPPER
builder.Services.AddAutoMapper(cfg =>
{
    cfg.AddMaps(typeof(TagProfile).Assembly);
});

// REGISTER SERVICES DI
builder.Services.AddScoped<ITagService, TagService>();
builder.Services.AddScoped<ISizeService, SizeService>();
builder.Services.AddScoped<IMaterialService, MaterialService>();
builder.Services.AddScoped<IColorService, ColorService>();
builder.Services.AddScoped<IClothingTypeService, ClothingTypeService>();
builder.Services.AddScoped<ICollectionService, CollectionService>();
builder.Services.AddScoped<IBrandService, BrandService>();
builder.Services.AddScoped<IClotheService, ClotheService>();
builder.Services.AddScoped<IClothesStockService, ClothesStockService>();

// REDIS
builder.Services.AddTransient<IEntityCacheInvalidationService<ClotheItem>, ClotheItemCacheInvalidationService>();
builder.Services.AddTransient<IEntityCacheInvalidationService<ClothesStock>, ClothesStockCacheInvalidationService>();

// CLOUDINARY CONFIG
builder.Services.AddCloudinary(builder.Configuration);

// FILTERS CACHE SERVICE
builder.Services.AddScoped<IFilterCacheInvalidationService, FilterCacheInvalidationService>();

// FLUENT VALIDATION
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

// OPEN TELEMETRY CONFIG
builder.Services.AddConfiguredOpenTelemetry("CatalogService", builder.Configuration);
Meter meter = builder.Services.AddOrGetMeter("CatalogService");
builder.Services.AddSingleton(meter);
//

var app = builder.Build();

app.MapGrpcService<OrderItemValidatorService>();
app.MapGrpcService<ClotheItemValidatorService>();
app.MapGrpcService<ClotheFilterService>();
app.MapGrpcService<GetClotheByIdGrpcService>();

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseServiceDefaults();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();