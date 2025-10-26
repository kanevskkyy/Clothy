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
    .WithHttpEndpoint(port: 5100, name: "catalog-http")
    .WithExternalHttpEndpoints()
    .WaitFor(postgresCatalog);

var seedCatalog = builder.AddProject<Clothy_CatalogService_SeedData>("catalog-seed")
    .WithReference(postgresCatalog)
    .WaitFor(catalogService);

var ordersService = builder.AddProject<Clothy_OrderService_API>("orders")
    .WithReference(postgresOrders)
    .WithHttpEndpoint(port: 5101, name: "orders-http")
    .WithExternalHttpEndpoints()
    .WaitFor(postgresOrders);

var seedOrders = builder.AddProject<Clothy_OrderService_SeedData>("order-seed")
    .WithReference(postgresOrders)
    .WaitFor(ordersService);

var reviewsService = builder.AddProject<Clothy_ReviewService_API>("reviews")
    .WithReference(mongo)
    .WithHttpEndpoint(port: 5102, name: "reviews-http")
    .WithExternalHttpEndpoints()
    .WaitFor(mongo);

var aggregator = builder.AddProject<Clothy_Aggregator>("aggregator")
    .WithReference(catalogService)
    .WithReference(ordersService)
    .WithReference(reviewsService)
    .WithReference(mongo)
    .WithHttpsEndpoint(port: 5200, name: "https")
    .WithExternalHttpEndpoints()
    .WaitFor(catalogService)
    .WaitFor(ordersService)
    .WaitFor(reviewsService)
    .WaitFor(mongo);

var gateway = builder.AddProject<Clothy_Gateway>("gateway")
    .WithReference(catalogService)
    .WithReference(ordersService)
    .WithReference(reviewsService)
    .WithReference(aggregator)
    .WithReference(mongo)
    .WithHttpsEndpoint(port: 7000, name: "https")
    .WithExternalHttpEndpoints()
    .WaitFor(aggregator);

var app = builder.Build();
await app.RunAsync();