using System.Text;
using Clothy.PaymentService.BLL.Config;
using Clothy.PaymentService.BLL.Services.Interfaces;
using Clothy.PaymentService.DAL.Context;
using Clothy.PaymentService.gRPC.Client.Services.Interfaces;
using MassTransit;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using Moq;
using Testcontainers.PostgreSql;
using Testcontainers.RabbitMq;
using Xunit;

namespace Clothy.PaymentService.IntegrationTests.Infrastructure;

public class PaymentServiceWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private PostgreSqlContainer postgresContainer;
    private RabbitMqContainer rabbitmqContainer;
    
    public Mock<IPaymentServiceFactory> PaymentServiceFactoryMock { get; } = new();
    public Mock<IGetOrderInfoClient> OrderInfoClientMock { get; } = new();
 
    public PaymentServiceWebApplicationFactory()
    {
        postgresContainer = new PostgreSqlBuilder()
            .WithImage("postgres:16-alpine")
            .WithDatabase("payment_test")
            .WithUsername("test")
            .WithPassword("test")
            .WithCleanUp(true)
            .Build();
 
        rabbitmqContainer = new RabbitMqBuilder()
            .WithImage("rabbitmq:3-management")
            .WithUsername("guest")
            .WithPassword("guest")
            .WithCleanUp(true)
            .Build();
    }
 
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("Jwt:Key", "SuperSecretKeyForTestingPurposesOnly12345678");
        builder.UseSetting("Jwt:Issuer", "ClothyTestIssuer");
        builder.UseSetting("Jwt:Audience", "ClothyTestAudience");
        
        builder.UseSetting("ConnectionStrings:ClothyPaymentDb", postgresContainer.GetConnectionString());
 
        builder.UseSetting("ConnectionStrings:rabbitmq",
            $"amqp://guest:guest@{rabbitmqContainer.Hostname}:{rabbitmqContainer.GetMappedPublicPort(5672)}/");
 
        builder.UseEnvironment("Testing");
 
        builder.ConfigureTestServices(services =>
        {
            var dbDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<PaymentDbContext>));
            if (dbDescriptor != null) services.Remove(dbDescriptor);
 
            var dbContextDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(PaymentDbContext));
            if (dbContextDescriptor != null) services.Remove(dbContextDescriptor);
 
            services.AddDbContext<PaymentDbContext>(options =>
                options.UseNpgsql(postgresContainer.GetConnectionString()));
            
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
 
            services.RemoveAll<IGetOrderInfoClient>();
            services.AddSingleton(OrderInfoClientMock.Object);
 
            services.RemoveAll<IBusControl>();
            services.AddMassTransitTestHarness(x =>
            {
                x.UsingInMemory((context, cfg) => cfg.ConfigureEndpoints(context));
            });
 
            services.RemoveAll<IHttpClientFactory>();
            services.AddSingleton<IHttpClientFactory>(new MockHttpClientFactory());
        });
    }
 
    public async Task InitializeAsync()
    {
        await postgresContainer.StartAsync();
        await rabbitmqContainer.StartAsync();
    
        using var scope = Services.CreateScope();
        PaymentDbContext db = scope.ServiceProvider.GetRequiredService<PaymentDbContext>();
        await db.Database.MigrateAsync();
    }
 
    public new async Task DisposeAsync()
    {
        await postgresContainer.DisposeAsync();
        await rabbitmqContainer.DisposeAsync();
        await base.DisposeAsync();
    }
}