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
using Clothy.ServiceDefaults.Middleware;
using Clothy.Shared.Cache.Interfaces;
using Clothy.Shared.Helpers;
using FluentValidation;
using Clothy.OrderService.gRPC.Client.Services.Interfaces;
using Clothy.OrderService.gRPC.Client.Services;
using FluentValidation.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddSingleton<IConnectionFactory>(sp =>
{
    string? connectionString = builder.Configuration.GetConnectionString("ClothyOrder");
    return new ConnectionFactory(connectionString);
});

builder.AddRedisClient("clothy-redis");
builder.Services.AddMemoryCache(options =>
{
    options.SizeLimit = 1024;

    options.CompactionPercentage = 0.2;
});

// REGISTER REPOSITORY
builder.Services.AddScoped<IOrderStatusRepository, OrderStatusRepository>();
builder.Services.AddScoped<IDeliveryProviderRepository, DeliveryProviderRepository>();
builder.Services.AddScoped<ICityRepository, CityRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IOrderItemRepository, OrderItemRepository>();
builder.Services.AddScoped<IDeliveryDetailRepository, DeliveryDetailRepository>();
builder.Services.AddScoped<IRegionRepository, RegionRepository>();

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
//

// FLUENT VALIDATION
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssembly(typeof(OrderStatusCreateDTOValidator).Assembly);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    string xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    string xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFilename);
    options.IncludeXmlComments(xmlPath);
});

//GRPC 
builder.Services.AddScoped<IOrderItemValidatorGrpcClient, OrderItemValidatorGrpcClient>();
builder.Services.AddConfiguredGrpcClient<OrderItemValidator.OrderItemValidatorClient>("catalog");
//

var app = builder.Build();

app.MapDefaultEndpoints();

await app.PreloadCachesAsync();

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseCorrelationId();
app.UseServiceLogging();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();