using System.Reflection;
using Clothy.OrderService.API.Middleware;
using Clothy.OrderService.BLL.FluentValidation.OrderStatusValidation;
using Clothy.OrderService.BLL.Interfaces;
using Clothy.OrderService.BLL.Mapper;
using Clothy.OrderService.BLL.Services;
using Clothy.OrderService.DAL.ConnectionFactory;
using Clothy.OrderService.DAL.Interfaces;
using Clothy.OrderService.DAL.Repositories;
using Clothy.OrderService.DAL.UOW;
using Clothy.ServiceDefaults.Middleware;
using Clothy.Shared.Helpers;
using DotNetEnv;
using FluentValidation;
using FluentValidation.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddSingleton<IConnectionFactory>(sp =>
{
    var connectionString = builder.Configuration.GetConnectionString("ClothyOrder");
    return new ConnectionFactory(connectionString);
});

// REGISTER REPOSITORY
builder.Services.AddScoped<IOrderStatusRepository, OrderStatusRepository>();
builder.Services.AddScoped<IDeliveryProviderRepository, DeliveryProviderRepository>();
builder.Services.AddScoped<ICityRepository, CityRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IOrderItemRepository, OrderItemRepository>();
builder.Services.AddScoped<IDeliveryDetailRepository, DeliveryDetailRepository>();

// REGISTER UNIT OF WORK
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// AUTO MAPPER REGISTER
builder.Services.AddAutoMapper(typeof(CityProfile).Assembly);

// SERVICES REGISTER
builder.Services.AddScoped<ICityService, CityService>();
builder.Services.AddScoped<IDeliveryProviderService, DeliveryProviderService>();
builder.Services.AddScoped<IOrderStatusService, OrderStatusService>();
builder.Services.AddScoped<IOrderService, OrderService>();

// CLOUDINARY CONFIG
builder.Services.AddCloudinary(builder.Configuration);
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

var app = builder.Build();

app.MapDefaultEndpoints();

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