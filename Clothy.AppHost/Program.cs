using Aspire.Hosting;
using Projects;

var builder = DistributedApplication.CreateBuilder(args);

var postgresPassword = builder.AddParameter("postgres-password");

var postgresCatalog = builder.AddPostgres("clothy-postgres-catalog", password: postgresPassword)
    .WithImage("postgres:16")
    .WithDataVolume("pgdata-catalog")
    .AddDatabase("ClothyCatalogDb");

var postgresOrders = builder.AddPostgres("clothy-postgres-orders", password: postgresPassword)
    .WithImage("postgres:16")
    .WithDataVolume("pgdata-orders")
    .WithInitBindMount("./Scripts")
    .AddDatabase("ClothyOrder");

var mongo = builder.AddMongoDB("clothy-mongo")
    .WithImage("mongo:7")
    .WithDataVolume("mongodata")
    .AddDatabase("ClothyReviewsDb");

var catalogService = builder.AddProject<Clothy_CatalogService_API>("catalog")
    .WithReference(postgresCatalog)
    .WaitFor(postgresCatalog);

var ordersService = builder.AddProject<Clothy_OrderService_API>("orders")
    .WithReference(postgresOrders)
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
    .WithReference(ordersService)
    .WithReference(reviewsService)
    .WaitFor(catalogService)
    .WaitFor(ordersService)
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