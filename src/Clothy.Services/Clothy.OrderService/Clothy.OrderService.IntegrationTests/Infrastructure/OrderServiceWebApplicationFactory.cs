using System.Text;
using Clothy.OrderService.gRPC.Client.Services.Interfaces;
using Clothy.Shared.Helpers.CloudinaryConfig.ImageService;
using MassTransit;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using Moq;
using Testcontainers.PostgreSql;
using Testcontainers.RabbitMq;
using Testcontainers.Redis;
using Xunit;

namespace Clothy.OrderService.IntegrationTests.Infrastructure;

public class OrderServiceWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private PostgreSqlContainer postgresContainer;
    private RabbitMqContainer rabbitmqContainer;
    private RedisContainer redisContainer;

    public Mock<IImageService> ImageServiceMock { get; } = new();
    public Mock<IOrderItemValidatorGrpcClient> OrderItemValidatorMock { get; } = new();
    public Mock<IBasketGrpcClient> BasketGrpcClientMock { get; } = new();
    
    public string PostgresConnectionString => postgresContainer.GetConnectionString();

    public OrderServiceWebApplicationFactory()
    {
        postgresContainer = new PostgreSqlBuilder()
            .WithImage("postgres:16")
            .WithCleanUp(true)
            .Build();

        rabbitmqContainer = new RabbitMqBuilder()
            .WithImage("rabbitmq:3-management")
            .WithUsername("guest")
            .WithPassword("guest")
            .WithCleanUp(true)
            .Build();

        redisContainer = new RedisBuilder()
            .WithImage("redis:7")
            .WithCleanUp(true)
            .Build();
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.ConfigureHostConfiguration(config =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:ClothyOrder"] = postgresContainer.GetConnectionString(),
                ["ConnectionStrings:clothy-redis"] = redisContainer.GetConnectionString() + ",allowAdmin=true",
                ["ConnectionStrings:rabbitmq"] =
                    $"amqp://guest:guest@{rabbitmqContainer.Hostname}:{rabbitmqContainer.GetMappedPublicPort(5672)}/",
                ["Jwt:Key"] = "SuperSecretKeyForTestingPurposesOnly12345678",
                ["Jwt:Issuer"] = "ClothyTestIssuer",
                ["Jwt:Audience"] = "ClothyTestAudience",
                ["Cloudinary:CloudName"] = "test",
                ["Cloudinary:ApiKey"] = "test",
                ["Cloudinary:ApiSecret"] = "test"
            });
        });

        return base.CreateHost(builder);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<IImageService>();
            services.AddSingleton(ImageServiceMock.Object);

            services.RemoveAll<IOrderItemValidatorGrpcClient>();
            services.AddSingleton(OrderItemValidatorMock.Object);

            services.RemoveAll<IBasketGrpcClient>();
            services.AddSingleton(BasketGrpcClientMock.Object);

            services.RemoveAll<IBusControl>();
            services.AddMassTransitTestHarness(x =>
            {
                x.UsingInMemory((context, cfg) => cfg.ConfigureEndpoints(context));
            });

            services.RemoveAll<IConfigureOptions<JwtBearerOptions>>();
            services.RemoveAll<IPostConfigureOptions<JwtBearerOptions>>();
            services.RemoveAll<IConfigureOptions<AuthenticationOptions>>();
            services.RemoveAll<IPostConfigureOptions<AuthenticationOptions>>();
            services.RemoveAll<IAuthenticationSchemeProvider>();
            services.RemoveAll<IAuthenticationHandlerProvider>();
            services.RemoveAll<IAuthenticationService>();
            services.RemoveAll<IClaimsTransformation>();

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = "ClothyTestIssuer",
                    ValidAudience = "ClothyTestAudience",
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes("SuperSecretKeyForTestingPurposesOnly12345678"))
                };
            });
        });
    }

    public async Task InitializeAsync()
    {
        await postgresContainer.StartAsync();
        await rabbitmqContainer.StartAsync();
        await redisContainer.StartAsync();
        
        await DatabaseInitializer.InitializeAsync(postgresContainer.GetConnectionString());

        _ = Services;
    }

    public async Task DisposeAsync()
    {
        await postgresContainer.DisposeAsync();
        await rabbitmqContainer.DisposeAsync();
        await redisContainer.DisposeAsync();
    }
}