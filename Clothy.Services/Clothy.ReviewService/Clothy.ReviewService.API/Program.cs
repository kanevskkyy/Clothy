using System.Reflection;
using System.Text.Json;
using Clothy.ReviewService.API.Middleware;
using Clothy.ReviewService.Application.Behaviours;
using Clothy.ReviewService.Application.Features.Questions.Commands.UpdateQuestion;
using Clothy.ReviewService.Application.Validations.Additional;
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
using Clothy.ReviewService.Infrastructure.Repositories;
using Clothy.ServiceDefaults.Middleware;
using FluentValidation;
using FluentValidation.AspNetCore;
using MediatR;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

ValueObjectMappings.Register();

builder.Services.AddMongoDb(builder.Configuration);

builder.Services.AddGrpc();

builder.Services.AddHealthChecks()
    .AddCheck<MongoHealthCheck>("MongoDB");

// MediatR
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<UpdateQuestionWithIdCommandHandler>());

// Behaviors
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ExceptionHandlingBehavior<,>));
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(PerformanceBehavior<,>));
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

// REPOSITORIES DI
builder.Services.AddScoped<IReviewRepository, ReviewRepository>();
builder.Services.AddScoped<IQuestionRepository, QuestionRepository>();

// FLUENT VALIDATION
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssembly(typeof(UpdateAnswerCommandValidator).Assembly);
//

builder.Services.AddEndpointsApiExplorer();

// GRPC
builder.Services.AddScoped<IClotheItemIdValidatorGrpcClient, ClotheItemIdValidatorGrpcClient>();
builder.Services.AddConfiguredGrpcClient<ClotheItemIdValidator.ClotheItemIdValidatorClient>("catalog")
    .AddStandardResilienceHandler(resilience =>
    {
        resilience.Retry.MaxRetryAttempts = 3;
        resilience.CircuitBreaker.FailureRatio = 0.4;
    });
//

var app = builder.Build();

app.MapDefaultEndpoints();

app.UseMiddleware<ExceptionHandlingMiddleware>();

//grpc server
app.MapGrpcService<ReviewServiceGrpcImpl>();
//

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
app.UseAuthorization();
app.MapControllers();

app.Run();