using System.Text;
using Clothy.BasketService.BLL.Consumers;
using Clothy.BasketService.gRPC.Client.Services.Interfaces;
using Clothy.OrderService.gRPC.Client.Services.Interfaces;
using Grpc.Net.Client;
using MassTransit;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Moq;
using StackExchange.Redis;
using Testcontainers.RabbitMq;
using Testcontainers.Redis;
using Xunit;

namespace Clothy.BasketService.ContractTests.Infrastructure;

public class BasketGrpcWebFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly RedisContainer redisContainer = new RedisBuilder()
        .WithImage("redis:7-alpine")
        .WithCleanUp(true)
        .Build();

    private readonly RabbitMqContainer rabbitmqContainer = new RabbitMqBuilder()
        .WithImage("rabbitmq:3-management")
        .WithUsername("guest")
        .WithPassword("guest")
        .WithCleanUp(true)
        .Build();

    public Mock<IOrderItemValidatorGrpcClient> OrderItemValidatorMock { get; } = new();
    public Mock<IOrderHistoryGrpcClient> OrderHistoryMock { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("ConnectionStrings:clothy-redis",
            redisContainer.GetConnectionString() + ",allowAdmin=true");
        builder.UseSetting("ConnectionStrings:rabbitmq",
            $"amqp://guest:guest@{rabbitmqContainer.Hostname}:{rabbitmqContainer.GetMappedPublicPort(5672)}/");
        builder.UseSetting("Jwt:Key", "SuperSecretKeyForTestingPurposesOnly12345678");
        builder.UseSetting("Jwt:Issuer", "ClothyTestIssuer");
        builder.UseSetting("Jwt:Audience", "ClothyTestAudience");

        builder.UseEnvironment("Testing");

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<IConnectionMultiplexer>();
            services.AddSingleton<IConnectionMultiplexer>(_ =>
                ConnectionMultiplexer.Connect(redisContainer.GetConnectionString() + ",allowAdmin=true"));

            services.RemoveAll<IOrderItemValidatorGrpcClient>();
            services.AddSingleton(OrderItemValidatorMock.Object);

            services.RemoveAll<IOrderHistoryGrpcClient>();
            services.AddSingleton(OrderHistoryMock.Object);

            services.RemoveAll<IBusControl>();
            services.AddMassTransitTestHarness(x =>
            {
                x.AddConsumer<UserDeletedConsumer>();
                x.UsingInMemory((ctx, cfg) => cfg.ConfigureEndpoints(ctx));
            });

            services.RemoveAll<IConfigureOptions<JwtBearerOptions>>();
            services.RemoveAll<IPostConfigureOptions<JwtBearerOptions>>();
            services.RemoveAll<IConfigureOptions<AuthenticationOptions>>();
            services.RemoveAll<IPostConfigureOptions<AuthenticationOptions>>();
            services.RemoveAll<IAuthenticationSchemeProvider>();
            services.RemoveAll<IAuthenticationHandlerProvider>();
            services.RemoveAll<IAuthenticationService>();
            services.RemoveAll<IClaimsTransformation>();

            ServiceDescriptor? schemeProvider = services.FirstOrDefault(d =>
                d.ServiceType == typeof(IAuthenticationSchemeProvider));
            if (schemeProvider != null) services.Remove(schemeProvider);

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

    public GrpcChannel CreateGrpcChannel() =>
        GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions
        {
            HttpHandler = Server.CreateHandler()
        });

    public async Task InitializeAsync()
    {
        await redisContainer.StartAsync();
        await rabbitmqContainer.StartAsync();
    }

    public new async Task DisposeAsync()
    {
        await redisContainer.DisposeAsync();
        await rabbitmqContainer.DisposeAsync();
        await base.DisposeAsync();
    }
}