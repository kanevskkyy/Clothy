using Projects;
using DotNetEnv;
using k8s.Models;

var builder = DistributedApplication.CreateBuilder(args);

string ENV_PATH = Path.Combine(Directory.GetCurrentDirectory(), ".env");
Env.Load(ENV_PATH);

var postgresPassword = builder.AddParameter("postgres-password", secret: true);

var rabbitMQUsername = builder.AddParameter("rabbitMq-username", secret: true);
var rabbitMQpassword = builder.AddParameter("rabbitMq-password", secret: true);

var keycloakAdminUsername = builder.AddParameter("keycloak-admin-username", secret: true);
var keycloakAdminPassword = builder.AddParameter("keycloak-admin-password", secret: true);

var postgres = builder.AddPostgres("clothy-postgres", password: postgresPassword)
    .WithImage("postgres:16")
    .WithDataVolume("pgdata")
    .WithBindMount("./Scripts", "/docker-entrypoint-initdb.d");

var redis = builder.AddRedis("clothy-redis")
    .WithDataVolume("redisdata");

var postgresCatalogDB = postgres.AddDatabase("ClothyCatalogDb");
var postgresOrdersDB = postgres.AddDatabase("ClothyOrder");
var postgresUsersDB = postgres.AddDatabase("ClothyUsers");


var mongo = builder.AddMongoDB("clothy-mongo")
    .WithImage("mongo:7")
    .WithDataVolume("mongodata")
    .AddDatabase("ClothyReviewsDb");

var rabbitmq = builder.AddRabbitMQ("rabbitmq", rabbitMQUsername, rabbitMQpassword)
    .WithManagementPlugin()
    .WithDataVolume();


var keycloak = builder.AddKeycloak("keycloak", port: 8080, keycloakAdminUsername, keycloakAdminPassword)
    .WithDataVolume();

var catalogService = builder.AddProject<Clothy_CatalogService_API>("catalog")
    .WithReference(postgresCatalogDB)
    .WithReference(redis)
    .WithReference(rabbitmq)
    .WithReference(keycloak)
    .WithEnvironment("CLOUDINARYSETTINGS__CLOUDNAME", Environment.GetEnvironmentVariable("CLOUDINARYSETTINGS__CLOUDNAME"))
    .WithEnvironment("CLOUDINARYSETTINGS__APIKEY", Environment.GetEnvironmentVariable("CLOUDINARYSETTINGS__APIKEY"))
    .WithEnvironment("CLOUDINARYSETTINGS__APISECRET", Environment.GetEnvironmentVariable("CLOUDINARYSETTINGS__APISECRET"))
    .WaitFor(redis)
    .WaitFor(rabbitmq)
    .WaitFor(postgresCatalogDB)
    .WaitFor(keycloak);

var basketService = builder.AddProject<Clothy_BasketService_API>("basket")
    .WithReference(redis)
    .WithReference(catalogService)
    .WithReference(rabbitmq)
    .WithReference(keycloak)
    .WaitFor(rabbitmq)
    .WaitFor(catalogService)
    .WaitFor(redis)
    .WaitFor(keycloak);

var ordersService = builder.AddProject<Clothy_OrderService_API>("orders")
    .WithReference(postgresOrdersDB)
    .WithReference(catalogService)
    .WithReference(basketService)
    .WithReference(redis)
    .WithReference(rabbitmq)
    .WithReference(keycloak)
    .WithEnvironment("CLOUDINARYSETTINGS__CLOUDNAME", Environment.GetEnvironmentVariable("CLOUDINARYSETTINGS__CLOUDNAME"))
    .WithEnvironment("CLOUDINARYSETTINGS__APIKEY", Environment.GetEnvironmentVariable("CLOUDINARYSETTINGS__APIKEY"))
    .WithEnvironment("CLOUDINARYSETTINGS__APISECRET", Environment.GetEnvironmentVariable("CLOUDINARYSETTINGS__APISECRET"))
    .WaitFor(redis)
    .WaitFor(basketService)
    .WaitFor(rabbitmq)
    .WaitFor(catalogService)
    .WaitFor(postgresOrdersDB)
    .WaitFor(keycloak);

var reviewsService = builder.AddProject<Clothy_ReviewService_API>("reviews")
    .WithReference(mongo)
    .WithReference(rabbitmq)
    .WithReference(catalogService)
    .WithReference(keycloak)
    .WaitFor(mongo)
    .WaitFor(rabbitmq)
    .WaitFor(catalogService)
    .WaitFor(keycloak);

// OLD USER SERVICE BEFORE KEYCLOAK

//var usersService = builder.AddProject<Clothy_UserService_API>("users")
//    .WithReference(postgresUsersDB)
//    .WithReference(rabbitmq)
//    .WithReference(keycloak)
//    .WithEnvironment("CLOUDINARYSETTINGS__CLOUDNAME", Environment.GetEnvironmentVariable("CLOUDINARYSETTINGS__CLOUDNAME"))
//    .WithEnvironment("CLOUDINARYSETTINGS__APIKEY", Environment.GetEnvironmentVariable("CLOUDINARYSETTINGS__APIKEY"))
//    .WithEnvironment("CLOUDINARYSETTINGS__APISECRET", Environment.GetEnvironmentVariable("CLOUDINARYSETTINGS__APISECRET"))
//    .WaitFor(rabbitmq)
//    .WaitFor(postgresUsersDB)
//    .WaitFor(keycloak);

//

var seedCatalog = builder.AddProject<Clothy_CatalogService_SeedData>("catalog-seed")
    .WithReference(postgresCatalogDB)
    .WaitFor(catalogService);

var seedOrders = builder.AddProject<Clothy_OrderService_SeedData>("order-seed")
    .WithReference(postgresOrdersDB)
    .WaitFor(ordersService);

var aggregator = builder.AddProject<Clothy_Aggregator_API>("aggregator")
    .WithReference(catalogService)
    .WithReference(redis)
    .WithReference(ordersService)
    .WithReference(basketService)
    .WithReference(reviewsService)
    .WithReference(keycloak)
    .WaitFor(catalogService)
    .WaitFor(ordersService)
    .WaitFor(redis)
    .WaitFor(basketService)
    .WaitFor(reviewsService)
    .WaitFor(keycloak);

var gateway = builder.AddProject<Clothy_Gateway>("gateway")
    .WithReference(catalogService)
    .WithReference(ordersService)
    .WithReference(reviewsService)
    .WithReference(basketService)
    .WithReference(aggregator)
    .WithReference(keycloak)
    .WithExternalHttpEndpoints()
    .WaitFor(aggregator);


var app = builder.Build();
await app.RunAsync();