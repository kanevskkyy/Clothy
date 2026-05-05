using System.Text;
using Clothy.ReviewService.gRPC.Client.Services.Interfaces;
using Grpc.Net.Client;
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
using Moq;
using Testcontainers.MongoDb;
using Testcontainers.RabbitMq;
using Xunit;

namespace Clothy.ReviewService.ContractTests.Infrastructure;

public class ReviewGrpcWebFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private MongoDbContainer mongoContainer = new MongoDbBuilder()
        .WithImage("mongo:7")
        .WithCleanUp(true)
        .Build();

    private RabbitMqContainer rabbitmqContainer = new RabbitMqBuilder()
        .WithImage("rabbitmq:3-management")
        .WithUsername("guest")
        .WithPassword("guest")
        .WithCleanUp(true)
        .Build();

    public Mock<IClotheItemIdValidatorGrpcClient> ClotheItemValidatorMock { get; } = new();
    public Mock<ICheckUserPurchasedClotheGrpcClient> CheckUserPurchasedMock { get; } = new();

    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.ConfigureHostConfiguration(config =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:ClothyReviewsDb"] = mongoContainer.GetConnectionString(),
                ["ConnectionStrings:rabbitmq"] =
                    $"amqp://guest:guest@{rabbitmqContainer.Hostname}:{rabbitmqContainer.GetMappedPublicPort(5672)}/",
                ["Jwt:Key"] = "SuperSecretKeyForTestingPurposesOnly12345678",
                ["Jwt:Issuer"] = "ClothyTestIssuer",
                ["Jwt:Audience"] = "ClothyTestAudience"
            });
        });
        return base.CreateHost(builder);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<IClotheItemIdValidatorGrpcClient>();
            services.AddSingleton(ClotheItemValidatorMock.Object);

            services.RemoveAll<ICheckUserPurchasedClotheGrpcClient>();
            services.AddSingleton(CheckUserPurchasedMock.Object);

            services.RemoveAll<IBusControl>();
            services.AddMassTransitTestHarness(x =>
                x.UsingInMemory((ctx, cfg) => cfg.ConfigureEndpoints(ctx)));

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

    public GrpcChannel CreateGrpcChannel() =>
        GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions
        {
            HttpHandler = Server.CreateHandler()
        });

    public async Task InitializeAsync()
    {
        await mongoContainer.StartAsync();
        await rabbitmqContainer.StartAsync();
    }

    public new async Task DisposeAsync()
    {
        await mongoContainer.DisposeAsync();
        await rabbitmqContainer.DisposeAsync();
    }
}