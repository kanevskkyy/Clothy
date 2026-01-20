using Aspire.Hosting;
using DotNetEnv;
using Projects;

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
var paymentDB = postgres.AddDatabase("ClothyPaymentDb");

//var postgresUsersDB = postgres.AddDatabase("ClothyUsers");

// Duende IdentityServer (Unused due to keycloak)
// var identityServerDb = postgres.AddDatabase("IdentityServerDb");
// 

var mongo = builder.AddMongoDB("clothy-mongo")
    .WithImage("mongo:7")
    .WithDataVolume("mongodata")
    .AddDatabase("ClothyReviewsDb");

var rabbitmq = builder.AddRabbitMQ("rabbitmq", rabbitMQUsername, rabbitMQpassword)
    .WithManagementPlugin()
    .WithDataVolume();

var keycloak = builder.AddKeycloak("keycloak", port: 8080, keycloakAdminUsername, keycloakAdminPassword)
    .WithDataVolume();

var authService = builder.AddProject<Clothy_AuthService_API>("auth")
    .WithReference(keycloak)
    .WithReference(rabbitmq)
    .WithEnvironment("CLOUDINARYSETTINGS__CLOUDNAME", Environment.GetEnvironmentVariable("CLOUDINARYSETTINGS__CLOUDNAME"))
    .WithEnvironment("CLOUDINARYSETTINGS__APIKEY", Environment.GetEnvironmentVariable("CLOUDINARYSETTINGS__APIKEY"))
    .WithEnvironment("CLOUDINARYSETTINGS__APISECRET", Environment.GetEnvironmentVariable("CLOUDINARYSETTINGS__APISECRET"))
    .WithEnvironment("KEYCLOAK__URL", Environment.GetEnvironmentVariable("KEYCLOAK__URL"))
    .WithEnvironment("KEYCLOAK__REALM", Environment.GetEnvironmentVariable("KEYCLOAK__REALM"))
    .WithEnvironment("KEYCLOAK__CLIENTID", Environment.GetEnvironmentVariable("KEYCLOAK__CLIENTID"))
    .WithEnvironment("KEYCLOAK__CLIENTSECRET", Environment.GetEnvironmentVariable("KEYCLOAK__CLIENTSECRET"))
    .WithEnvironment("FRONTEND__URL", Environment.GetEnvironmentVariable("FRONTEND__URL"))
    .WaitFor(rabbitmq)
    .WaitFor(keycloak);

// UNUSED
//var identityServer = builder.AddProject<Clothy_IdentityServer_API>("identity")
//    .WithReference(identityServerDb)
//    .WaitFor(identityServerDb);
//

var catalogService = builder.AddProject<Clothy_CatalogService_API>("catalog")
    .WithReference(postgresCatalogDB)
    .WithReference(redis)
    .WithReference(rabbitmq)
    .WithReference(keycloak)
    .WithEnvironment("CLOUDINARYSETTINGS__CLOUDNAME", Environment.GetEnvironmentVariable("CLOUDINARYSETTINGS__CLOUDNAME"))
    .WithEnvironment("CLOUDINARYSETTINGS__APIKEY", Environment.GetEnvironmentVariable("CLOUDINARYSETTINGS__APIKEY"))
    .WithEnvironment("CLOUDINARYSETTINGS__APISECRET", Environment.GetEnvironmentVariable("CLOUDINARYSETTINGS__APISECRET"))
    .WithEnvironment("FRONTEND__URL", Environment.GetEnvironmentVariable("FRONTEND__URL"))
    .WaitFor(redis)
    .WaitFor(rabbitmq)
    .WaitFor(postgresCatalogDB)
    .WaitFor(keycloak);

var notificationService = builder.AddProject<Clothy_NotificationService_API>("notification")
    .WithReference(rabbitmq)
    .WithEnvironment("SENDGRID__KEY", Environment.GetEnvironmentVariable("SENDGRID__KEY"))
    .WithEnvironment("SENDGRID__FROM_EMAIL", Environment.GetEnvironmentVariable("SENDGRID__FROM_EMAIL"))
    .WithEnvironment("SENDGRID__FROM_NAME", Environment.GetEnvironmentVariable("SENDGRID__FROM_NAME"))
    .WaitFor(rabbitmq);

var basketService = builder.AddProject<Clothy_BasketService_API>("basket")
    .WithReference(redis)
    .WithReference(catalogService)
    .WithReference(rabbitmq)
    .WithReference(keycloak)
    .WithEnvironment("FRONTEND__URL", Environment.GetEnvironmentVariable("FRONTEND__URL"))
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
    .WithEnvironment("NOVAPOSHTA__API_KEY", Environment.GetEnvironmentVariable("NOVAPOSHTA__API_KEY"))
    .WithEnvironment("FRONTEND__URL", Environment.GetEnvironmentVariable("FRONTEND__URL"))
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
    .WithReference(ordersService)
    .WithReference(keycloak)
    .WithEnvironment("FRONTEND__URL", Environment.GetEnvironmentVariable("FRONTEND__URL"))
    .WaitFor(mongo)
    .WaitFor(rabbitmq)
    .WaitFor(catalogService)
    .WaitFor(ordersService)
    .WaitFor(keycloak);

var paymentService = builder.AddProject<Clothy_PaymentService_API>("payments")
    .WithReference(paymentDB)
    .WithEnvironment("STRIPE__SECRET_KEY", Environment.GetEnvironmentVariable("STRIPE__SECRET_KEY"))
    .WithEnvironment("STRIPE__PUBLISHABLE_KEY", Environment.GetEnvironmentVariable("STRIPE__PUBLISHABLE_KEY"))
    .WithEnvironment("STRIPE__WEBHOOK_SECRET", Environment.GetEnvironmentVariable("STRIPE__WEBHOOK_SECRET"))
    .WithEnvironment("NOWPAYMENTS__API_KEY", Environment.GetEnvironmentVariable("NOWPAYMENTS__API_KEY"))
    .WithEnvironment("NOWPAYMENTS__CALLBACK_URL", Environment.GetEnvironmentVariable("NOWPAYMENTS__CALLBACK_URL"))
    .WithEnvironment("NOWPAYMENTS__WEBHOOK_SECRET", Environment.GetEnvironmentVariable("NOWPAYMENTS__WEBHOOK_SECRET"))
    .WithEnvironment("SUCCESS__URL", Environment.GetEnvironmentVariable("SUCCESS__URL"))
    .WithEnvironment("CANCEL__URL", Environment.GetEnvironmentVariable("CANCEL__URL"))
    .WithEnvironment("FRONTEND__URL", Environment.GetEnvironmentVariable("FRONTEND__URL"))
    .WithReference(rabbitmq)
    .WithReference(ordersService)
    .WithReference(keycloak)
    .WaitFor(rabbitmq)
    .WaitFor(ordersService)
    .WaitFor(keycloak)
    .WaitFor(paymentDB);


// OLD USER SERVICE BEFORE KEYCLOAK

//var usersService = builder.AddProject<Clothy_UserService_API>("users")
//    .WithReference(postgresUsersDB)
//    .WithReference(rabbitmq)
//    .WithReference(keycloak)
//    .WithEnvironment("CLOUDINARYSETTINGS__CLOUDNAME", Environment.GetEnvironmentVariable("CLOUDINARYSETTINGS__CLOUDNAME"))
//    .WithEnvironment("CLOUDINARYSETTINGS__APIKEY", Environment.GetEnvironmentVariable("CLOUDINARYSETTINGS__APIKEY"))
//    .WithEnvironment("CLOUDINARYSETTINGS__APISECRET", Environment.GetEnvironmentVariable("CLOUDINARYSETTINGS__APISECRET"))
//    .WithEnvironment("FRONTEND__URL", Environment.GetEnvironmentVariable("FRONTEND__URL"))
//    .WaitFor(rabbitmq)
//    .WaitFor(postgresUsersDB)
//    .WaitFor(keycloak);

//

var seedCatalog = builder.AddProject<Clothy_CatalogService_SeedData>("catalog-seed")
    .WithReference(postgresCatalogDB)
    .WaitFor(catalogService);

var seedOrders = builder.AddProject<Clothy_OrderService_SeedData>("order-seed")
    .WithReference(postgresOrdersDB)
    .WithEnvironment("NOVAPOSHTA__API_KEY", Environment.GetEnvironmentVariable("NOVAPOSHTA__API_KEY"))
    .WaitFor(ordersService);

var aggregator = builder.AddProject<Clothy_Aggregator_API>("aggregator")
    .WithReference(catalogService)
    .WithReference(redis)
    .WithReference(ordersService)
    .WithReference(basketService)
    .WithReference(reviewsService)
    .WithReference(keycloak)
    .WithEnvironment("FRONTEND__URL", Environment.GetEnvironmentVariable("FRONTEND__URL"))
    .WaitFor(catalogService)
    .WaitFor(ordersService)
    .WaitFor(authService)
    .WaitFor(redis)
    .WaitFor(basketService)
    .WaitFor(reviewsService)
    .WaitFor(authService)
    .WaitFor(keycloak);

var gateway = builder.AddProject<Clothy_Gateway>("gateway")
    .WithReference(catalogService)
    .WithReference(ordersService)
    .WithReference(reviewsService)
    .WithReference(paymentService)
    .WithReference(basketService)
    .WithReference(aggregator)
    .WithReference(authService)
    .WithEnvironment("FRONTEND__URL", Environment.GetEnvironmentVariable("FRONTEND__URL"))
    .WithExternalHttpEndpoints()
    .WaitFor(aggregator);

var app = builder.Build();
await app.RunAsync();