using Clothy.PaymentService.API.Middleware;
using Clothy.PaymentService.BLL.Config;
using Clothy.PaymentService.BLL.Fraud;
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
using Microsoft.Extensions.Http.Resilience;
using Microsoft.Extensions.Options;
using Polly;

var builder = WebApplication.CreateBuilder(args);

if (builder.Environment.IsDevelopment()) Env.Load();

builder.AddServiceDefaults();

string? paymentConnStr = builder.Configuration.GetConnectionString("ClothyPaymentDb");
if (!string.IsNullOrEmpty(paymentConnStr))
    builder.Services.AddDbContext<PaymentDbContext>(options =>
        options.UseNpgsql(paymentConnStr));
else builder.AddNpgsqlDbContext<PaymentDbContext>("ClothyPaymentDb");

builder.Services.AddGrpc();
builder.Services.AddScoped<IGetOrderInfoClient, GetOrderInfoClient>();
builder.Services.AddConfiguredGrpcClient<OrderServiceGrpc.OrderServiceGrpcClient>("orders")
    .AddStandardResilienceHandler(resilience =>
    {
        resilience.Retry.MaxRetryAttempts = 3;
        resilience.CircuitBreaker.FailureRatio = 0.3;
    });

builder.Services.Configure<CardSettings>(options =>
{
    options.ApiKey = Environment.GetEnvironmentVariable("STRIPE__SECRET_KEY");
    options.PublishableKey = Environment.GetEnvironmentVariable("STRIPE__PUBLISHABLE_KEY");
    options.SuccessURL = Environment.GetEnvironmentVariable("SUCCESS__URL");
    options.CancelURL = Environment.GetEnvironmentVariable("CANCEL__URL");
    options.WebhookSecret = Environment.GetEnvironmentVariable("STRIPE__WEBHOOK_SECRET");
});

builder.Services.Configure<CryptoSettings>(
    builder.Configuration.GetSection("CryptoPayment")
);

builder.Services.PostConfigure<CryptoSettings>(options =>
{
    options.ApiKey = Environment.GetEnvironmentVariable("NOWPAYMENTS__API_KEY") ?? options.ApiKey;
    options.WebhookSecret = Environment.GetEnvironmentVariable("NOWPAYMENTS__WEBHOOK_SECRET") ?? options.WebhookSecret;
    options.CallbackURL = Environment.GetEnvironmentVariable("NOWPAYMENTS__CALLBACK_URL") ?? options.CallbackURL;
    options.SuccessURL = Environment.GetEnvironmentVariable("SUCCESS__URL") ?? options.SuccessURL;
    options.CancelURL = Environment.GetEnvironmentVariable("CANCEL__URL") ?? options.CancelURL;
});

builder.Services.AddHttpClient("NowPayments");

// Fraud scoring (Clothy.AI.FraudService). Payments never depend on it: the client returns "not scored"
// on any failure, FraudSettings.Timeout (2 s) is a hard limit per call, and the circuit breaker stops
// calling a service that keeps failing.
builder.Services.Configure<FraudSettings>(options =>
{
    options.BaseUrl = Environment.GetEnvironmentVariable("FRAUD__URL") ?? options.BaseUrl;
});
builder.Services.AddHttpClient<IFraudScoringClient, FraudScoringClient>((sp, client) =>
    {
        FraudSettings settings = sp.GetRequiredService<IOptions<FraudSettings>>().Value;
        client.BaseAddress = new Uri(settings.BaseUrl.TrimEnd('/') + "/");
    })
    .AddResilienceHandler("fraud", pipeline =>
    {
        pipeline.AddCircuitBreaker(new HttpCircuitBreakerStrategyOptions
        {
            FailureRatio = 0.5,
            MinimumThroughput = 5,
            SamplingDuration = TimeSpan.FromSeconds(30),
            BreakDuration = TimeSpan.FromSeconds(30)
        });
        pipeline.AddTimeout(TimeSpan.FromSeconds(2));
    });
builder.Services.AddScoped<IStripePaymentDetailsProvider, StripePaymentDetailsProvider>();
builder.Services.AddScoped<IStripeFraudScorer, StripeFraudScorer>();

builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssembly(typeof(CreatePaymentRequestDTOValidator).Assembly);

builder.Services.AddScoped<IPaymentService, StripeService>();
builder.Services.AddScoped<IPaymentService, NowPaymentsService>();
builder.Services.AddScoped<IPaymentServiceFactory, PaymentServiceFactory>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

await using (var scope = builder.Services.BuildServiceProvider().CreateAsyncScope())
{
    PaymentDbContext dbContext = scope.ServiceProvider.GetRequiredService<PaymentDbContext>();
    await dbContext.Database.MigrateAsync();
}

builder.Services.AddMassTransit(x =>
{
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(builder.Configuration.GetConnectionString("rabbitmq"));
        cfg.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(5)));

        cfg.Message<OrderPaidEvent>(e => e.SetEntityName("order-paid"));
    });
});

var app = builder.Build();
app.UseServiceDefaults();

app.MapDefaultEndpoints();

app.UseMiddleware<ExceptionHandlingMiddleware>();

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

public partial class Program
{
    
}