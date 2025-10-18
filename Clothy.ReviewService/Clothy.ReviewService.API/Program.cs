using Clothy.ReviewService.Infrastructure.DB.Extension;
using Clothy.ReviewService.Infrastructure.DB.MongoHeathCheck;
using Clothy.ReviewService.Infrastructure.DB.Seeding;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMongoDb(builder.Configuration);

builder.Services.AddHealthChecks()
    .AddCheck<MongoHealthCheck>("MongoDB");

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