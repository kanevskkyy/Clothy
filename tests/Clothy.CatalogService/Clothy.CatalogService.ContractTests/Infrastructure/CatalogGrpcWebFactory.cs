using System.Text;
using Clothy.CatalogService.DAL.DB;
using Clothy.Shared.Helpers.CloudinaryConfig.ImageService;
using Grpc.Net.Client;
using MassTransit;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Moq;
using StackExchange.Redis;
using Testcontainers.PostgreSql;
using Testcontainers.RabbitMq;
using Testcontainers.Redis;
using Xunit;

namespace Clothy.CatalogService.ContractTests.Infrastructure;

public class CatalogGrpcWebFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private PostgreSqlContainer postgresContainer = new PostgreSqlBuilder()
        .WithImage("postgres:16")
        .WithCleanUp(true)
        .Build();

    private RabbitMqContainer rabbitmqContainer = new RabbitMqBuilder()
        .WithImage("rabbitmq:3-management")
        .WithUsername("guest")
        .WithPassword("guest")
        .WithCleanUp(true)
        .Build();

    private RedisContainer redisContainer = new RedisBuilder()
        .WithImage("redis:7")
        .WithCleanUp(true)
        .Build();

    public Mock<IImageService> ImageServiceMock { get; } = new();

    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.ConfigureHostConfiguration(config =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:ClothyCatalogDb"] = postgresContainer.GetConnectionString(),
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

            services.RemoveAll<IBusControl>();
            services.AddMassTransitTestHarness(x =>
                x.UsingInMemory((ctx, cfg) => cfg.ConfigureEndpoints(ctx)));

            services.RemoveAll<IConnectionMultiplexer>();
            services.AddSingleton<IConnectionMultiplexer>(_ =>
                ConnectionMultiplexer.Connect(redisContainer.GetConnectionString() + ",allowAdmin=true"));

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
        await postgresContainer.StartAsync();
        await rabbitmqContainer.StartAsync();
        await redisContainer.StartAsync();

        using IServiceScope scope = Services.CreateScope();
        ClothyCatalogDbContext db = scope.ServiceProvider.GetRequiredService<ClothyCatalogDbContext>();
        await db.Database.MigrateAsync();
    }

    public new async Task DisposeAsync()
    {
        await postgresContainer.DisposeAsync();
        await rabbitmqContainer.DisposeAsync();
        await redisContainer.DisposeAsync();
    }
}