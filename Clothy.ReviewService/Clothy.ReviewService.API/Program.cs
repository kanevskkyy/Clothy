using Clothy.ReviewService.Application.Behaviours;
using Clothy.ReviewService.Application.Features.Questions.Commands.UpdateQuestion;
using Clothy.ReviewService.Application.Services;
using Clothy.ReviewService.Domain.Interfaces.Repositories;
using Clothy.ReviewService.Domain.Interfaces.Services;
using Clothy.ReviewService.Infrastructure.DB.Extension;
using Clothy.ReviewService.Infrastructure.DB.MappingConfig;
using Clothy.ReviewService.Infrastructure.DB.MongoHeathCheck;
using Clothy.ReviewService.Infrastructure.DB.Seeding;
using Clothy.ReviewService.Infrastructure.Repositories;
using MediatR;

var builder = WebApplication.CreateBuilder(args);

ValueObjectMappings.Register();

builder.Services.AddMongoDb(builder.Configuration);

// MONGO HEALTH CHECK
builder.Services.AddHealthChecks()
    .AddCheck<MongoHealthCheck>("MongoDB");
//

//MediatR
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<UpdateQuestionCommandHandler>());
//

// Behaviors
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ExceptionHandlingBehavior<,>));
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(PerformanceBehavior<,>));
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
//

// REPOSITORIES DI
builder.Services.AddScoped<IReviewRepository, ReviewRepository>();
builder.Services.AddScoped<IQuestionRepository, QuestionRepository>();
//

// SERVICES DI
builder.Services.AddScoped<IReviewService, ReviewService>();
builder.Services.AddScoped<IQuestionService, QuestionService>();
//

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    services.EnsureIndexes();

    ReviewSeeder reviewSeeder = services.GetRequiredService<ReviewSeeder>();
    await reviewSeeder.SeedAsync();

    QuestionSeeder questionSeeder = services.GetRequiredService<QuestionSeeder>();
    await questionSeeder.SeedAsync();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.MapHealthChecks("/health");

app.Run();