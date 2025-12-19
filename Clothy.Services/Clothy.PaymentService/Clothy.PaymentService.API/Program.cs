using Clothy.PaymentService.API.Middleware;
using Clothy.PaymentService.BLL.Config;
using Clothy.PaymentService.BLL.Services;
using Clothy.PaymentService.BLL.Services.Interfaces;
using Clothy.PaymentService.BLL.Validation;
using Clothy.PaymentService.DAL.Context;
using Clothy.PaymentService.gRPC.Client.Services;
using Clothy.PaymentService.gRPC.Client.Services.Interfaces;
using Clothy.ServiceDefaults.Middleware.Grpc;
using Clothy.Shared.Events.PaymentEvents;
using DotNetEnv;
using FluentValidation;
using FluentValidation.AspNetCore;
using MassTransit;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

Env.Load();

builder.AddServiceDefaults();

builder.AddNpgsqlDbContext<PaymentDbContext>("ClothyPaymentDb");

builder.Services.AddGrpc();
builder.Services.AddScoped<IGetOrderInfoClient, GetOrderInfoClient>();
builder.Services.AddConfiguredGrpcClient<OrderServiceGrpc.OrderServiceGrpcClient>("orders")
    .AddStandardResilienceHandler(resilience =>
    {
        resilience.Retry.MaxRetryAttempts = 3;
        resilience.CircuitBreaker.FailureRatio = 0.3;
    });

builder.Services.Configure<PaymentSettings>(options =>
{
    options.SecretKey = Environment.GetEnvironmentVariable("STRIPE__SECRET_KEY");
    options.PublishableKey = Environment.GetEnvironmentVariable("STRIPE__PUBLISHABLE_KEY");
    options.SuccessUrl = Environment.GetEnvironmentVariable("STRIPE__SUCCESS_URL");
    options.CancelUrl = Environment.GetEnvironmentVariable("STRIPE__CANCEL_URL");
    options.WebhookSecret = Environment.GetEnvironmentVariable("STRIPE__WEBHOOK_SECRET");
});


// FLUENT VALIDATION
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssembly(typeof(CreatePaymentRequestDTOValidator).Assembly);
//

builder.Services.AddScoped<IPaymentService, StripeService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

await using (var scope = builder.Services.BuildServiceProvider().CreateAsyncScope())
{
    PaymentDbContext dbContext = scope.ServiceProvider.GetRequiredService<PaymentDbContext>();
    await dbContext.Database.MigrateAsync();
}

//MassTransit
builder.Services.AddMassTransit(x =>
{
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(builder.Configuration.GetConnectionString("rabbitmq"));
        cfg.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(5)));

        cfg.Message<OrderPaidEvent>(e => e.SetEntityName("order-paid"));
    });
});
//


var app = builder.Build();
app.UseServiceDefaults();

app.MapDefaultEndpoints();

app.UseMiddleware<ExceptionHandlingMiddleware>();

// Configure the HTTP request pipeline.
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
