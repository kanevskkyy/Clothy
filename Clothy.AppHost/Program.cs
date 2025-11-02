using Aspire.Hosting;
using Projects;
using DotNetEnv;

var builder = DistributedApplication.CreateBuilder(args);

string ENV_PATH = Path.Combine(Directory.GetCurrentDirectory(), ".env");
if (File.Exists(ENV_PATH))
{
    Env.Load(ENV_PATH);
}

var postgresPassword = builder.AddParameter("postgres-password");

var postgres = builder.AddPostgres("clothy-postgres", password: postgresPassword)
    .WithImage("postgres:16")
    .WithDataVolume("pgdata")
    .WithBindMount("./Scripts", "/docker-entrypoint-initdb.d");

var redis = builder.AddRedis("clothy-redis")
    .WithDataVolume("redisdata");

var postgresCatalog = postgres.AddDatabase("ClothyCatalogDb");
var postgresOrders = postgres.AddDatabase("ClothyOrder");

var mongo = builder.AddMongoDB("clothy-mongo")
    .WithImage("mongo:7")
    .WithDataVolume("mongodata")
    .AddDatabase("ClothyReviewsDb");

var catalogService = builder.AddProject<Clothy_CatalogService_API>("catalog")
    .WithReference(postgresCatalog)
    .WithReference(redis)
    .WithEnvironment("CLOUDINARYSETTINGS__CLOUDNAME", Environment.GetEnvironmentVariable("CLOUDINARYSETTINGS__CLOUDNAME"))
    .WithEnvironment("CLOUDINARYSETTINGS__APIKEY", Environment.GetEnvironmentVariable("CLOUDINARYSETTINGS__APIKEY"))
    .WithEnvironment("CLOUDINARYSETTINGS__APISECRET", Environment.GetEnvironmentVariable("CLOUDINARYSETTINGS__APISECRET"))
    .WaitFor(redis)
    .WaitFor(postgresCatalog);

var ordersService = builder.AddProject<Clothy_OrderService_API>("orders")
    .WithReference(postgresOrders)
    .WithReference(redis)
    .WithEnvironment("CLOUDINARYSETTINGS__CLOUDNAME", Environment.GetEnvironmentVariable("CLOUDINARYSETTINGS__CLOUDNAME"))
    .WithEnvironment("CLOUDINARYSETTINGS__APIKEY", Environment.GetEnvironmentVariable("CLOUDINARYSETTINGS__APIKEY"))
    .WithEnvironment("CLOUDINARYSETTINGS__APISECRET", Environment.GetEnvironmentVariable("CLOUDINARYSETTINGS__APISECRET"))
    .WaitFor(redis)
    .WaitFor(postgresOrders);

var reviewsService = builder.AddProject<Clothy_ReviewService_API>("reviews")
    .WithReference(mongo)
    .WaitFor(mongo);

var seedCatalog = builder.AddProject<Clothy_CatalogService_SeedData>("catalog-seed")
    .WithReference(postgresCatalog)
    .WaitFor(catalogService);

var seedOrders = builder.AddProject<Clothy_OrderService_SeedData>("order-seed")
    .WithReference(postgresOrders)
    .WaitFor(ordersService);

var aggregator = builder.AddProject<Clothy_Aggregator>("aggregator")
    .WithReference(catalogService)
    .WithReference(redis)
    .WithReference(ordersService)
    .WithReference(reviewsService)
    .WaitFor(catalogService)
    .WaitFor(ordersService)
    .WaitFor(redis)
    .WaitFor(reviewsService);

var gateway = builder.AddProject<Clothy_Gateway>("gateway")
    .WithReference(catalogService)
    .WithReference(ordersService)
    .WithReference(reviewsService)
    .WithReference(aggregator)
    .WithExternalHttpEndpoints()
    .WaitFor(aggregator);

var app = builder.Build();
await app.RunAsync();