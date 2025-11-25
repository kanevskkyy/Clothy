using Clothy.BasketService.DAL.Repositories.Interfaces;
using Clothy.BasketService.DAL.Repositories;
using Clothy.BasketService.BLL.Services.Interfaces;
using Clothy.BasketService.BLL.Services;
using Clothy.OrderService.gRPC.Client.Services.Interfaces;
using Clothy.OrderService.gRPC.Client.Services;
using Clothy.ServiceDefaults.Middleware;
using Clothy.BasketService.API.Middleware;
using Clothy.BasketService.BLL.Mapper;
using Clothy.BasketService.BLL.DTOs;
using Clothy.BasketService.BLL.Validation;
using FluentValidation.AspNetCore;
using FluentValidation;
using Clothy.BasketService.gRPC.Server.Server;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.AddRedisClient("clothy-redis");

builder.Services.AddScoped<IBasketRepository, BasketRepository>();
builder.Services.AddScoped<IBasketService, BasketService>();

builder.Services.AddGrpc();

//GRPC
builder.Services.AddScoped<IOrderItemValidatorGrpcClient, OrderItemValidatorGrpcClient>();
builder.Services.AddConfiguredGrpcClient<OrderItemValidator.OrderItemValidatorClient>("catalog")
    .AddStandardResilienceHandler(resilience =>
    {
        resilience.Retry.MaxRetryAttempts = 3;
        resilience.CircuitBreaker.FailureRatio = 0.3;
    });

//AUTOMAPPER
builder.Services.AddAutoMapper(typeof(BasketProfile));

// FLUENT VALIDATION
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssembly(typeof(BasketItemCreateDTOValidator).Assembly);

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

//Grpc server
app.MapGrpcService<BasketGrpcService>();
//

app.UseServiceDefaults();
app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
