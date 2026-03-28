using Clothy.ReviewService.API.Middleware;
using Clothy.ReviewService.Application.Behaviours;
using Clothy.ReviewService.Application.Consumers;
using Clothy.ReviewService.Application.Consumers.DeleteReviewsAndQuestions;
using Clothy.ReviewService.Application.Features.Questions.Commands.UpdateQuestion;
using Clothy.ReviewService.Application.Validations.Questions;
using Clothy.ReviewService.Application.Validations.Reviews;
using Clothy.ReviewService.Domain.Interfaces;
using Clothy.ReviewService.gRPC.Client.Services;
using Clothy.ReviewService.gRPC.Client.Services.Interfaces;
using Clothy.ReviewService.gRPC.Server.Services;
using Clothy.ReviewService.Infrastructure.DB.Extension;
using Clothy.ReviewService.Infrastructure.DB.MappingConfig;
using Clothy.ReviewService.Infrastructure.DB.MongoHeathCheck;
using Clothy.ReviewService.Infrastructure.DB.Seeding;
using Clothy.ReviewService.Infrastructure.EventLog;
using Clothy.ReviewService.Infrastructure.Repositories;
using Clothy.ServiceDefaults.Middleware.Grpc;
using Clothy.ServiceDefaults.Middleware.OpenTelemetry;
using Clothy.Shared.Events;
using FluentValidation;
using FluentValidation.AspNetCore;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Diagnostics.Metrics;
using System.Reflection;
using System.Text.Json;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHealthChecks()
    .AddCheck<MongoHealthCheck>(
        name: "mongodb",
        failureStatus: HealthStatus.Unhealthy,
        tags: new[] { "ready", "db", "mongodb", "nosql" }
    );

builder.AddServiceDefaults();

ValueObjectMappings.Register();

builder.Services.AddMongoDb(builder.Configuration);

builder.Services.AddGrpc();

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<UpdateQuestionWithIdCommandHandler>());

builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ExceptionHandlingBehavior<,>));
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(PerformanceBehavior<,>));
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

builder.Services.AddScoped<IReviewRepository, ReviewRepository>();
builder.Services.AddScoped<IQuestionRepository, QuestionRepository>();

builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssembly(typeof(CreateQuestionDTOValidator).Assembly);

builder.Services.AddScoped<IEventLogService, EventLogService>();

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<ReviewsAndQuestionsDeletionConsumer>();
    x.AddConsumer<UserDeletedConsumer>();
    x.AddConsumer<UserUpdatedConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(builder.Configuration.GetConnectionString("rabbitmq"));
        cfg.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(5)));

        cfg.ReceiveEndpoint("review-service-user-updated-queue", e =>
        {
            e.ConfigureConsumer<UserUpdatedConsumer>(context);
            e.Bind("user-updated-event");
        });

        cfg.ReceiveEndpoint("review-service-clothe-deleted-queue", e =>
        {
            e.ConfigureConsumer<ReviewsAndQuestionsDeletionConsumer>(context);
            e.Bind("clothe-item-deleted");
        });

        cfg.ReceiveEndpoint("review-service-user-delete-queue", e =>
        {
            e.ConfigureConsumer<UserDeletedConsumer>(context);
            e.Bind("user-deleted");
        });
    });
});

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddScoped<ICheckUserPurchasedClotheGrpcClient, CheckUserPurchasedClotheGrpcClient>();
builder.Services.AddConfiguredGrpcClient<CheckUserPurchasedGrpc.CheckUserPurchasedGrpcClient>("orders")
    .AddStandardResilienceHandler(resilience =>
    {
        resilience.Retry.MaxRetryAttempts = 3;
        resilience.CircuitBreaker.FailureRatio = 0.4;
    });

builder.Services.AddScoped<IClotheItemIdValidatorGrpcClient, ClotheItemIdValidatorGrpcClient>();
builder.Services.AddConfiguredGrpcClient<ClotheItemIdValidator.ClotheItemIdValidatorClient>("catalog")
    .AddStandardResilienceHandler(resilience =>
    {
        resilience.Retry.MaxRetryAttempts = 3;
        resilience.CircuitBreaker.FailureRatio = 0.4;
    });

builder.Services.AddConfiguredOpenTelemetry("ReviewService", builder.Configuration);
Meter meter = builder.Services.AddOrGetMeter("ReviewService");
builder.Services.AddSingleton(meter);

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.MapGrpcService<ReviewServiceGrpcImpl>();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    services.EnsureIndexes();
    List<IDataSeeder> seeders = new List<IDataSeeder>()
    {
        services.GetRequiredService<ReviewSeeder>(),
        services.GetRequiredService<QuestionSeeder>()
    };
    foreach (var seeder in seeders)
    {
        await seeder.SeedAsync();
    }
}

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