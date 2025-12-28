using Clothy.OrderService.BLL.Config;
using Clothy.OrderService.BLL.Interfaces;
using Clothy.OrderService.BLL.Services;
using Clothy.OrderService.DAL.ConnectionFactory;
using Clothy.OrderService.DAL.Interfaces;
using Clothy.OrderService.DAL.Repositories;
using Clothy.OrderService.DAL.UOW;
using Clothy.OrderService.SeedData.Seeders;
using DotNetEnv;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

class Program
{
    public static async Task Main()
    {
        Env.Load();

        Console.WriteLine("Starting seed...");

        var builder = new ConfigurationBuilder()
                    .AddEnvironmentVariables()
                    .Build();

        string? connectionString = builder.GetConnectionString("ClothyOrder");
        Console.WriteLine($"Using connection string: {connectionString}");

        ServiceCollection services = new ServiceCollection();
        services.AddSingleton<IConnectionFactory>(provider => new ConnectionFactory(connectionString!));
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IOrderStatusRepository, OrderStatusRepository>();
        services.AddScoped<IDeliveryProviderRepository, DeliveryProviderRepository>();
        services.AddScoped<IOrderItemRepository, OrderItemRepository>();
        services.AddScoped<IRegionRepository, RegionRepository>();
        services.AddScoped<ISettlementRepository, SettlementRepository>();
        services.AddScoped<IPickupPointRepository, PickupPointRepository>();
        services.AddScoped<IDeliveryDetailRepository, DeliveryDetailRepository>();
        services.AddScoped<IOrderReservationRepository, OrderReservationRepository>();

        services.AddHttpClient("NovaPoshtaAPI", client =>
        {
            client.Timeout = TimeSpan.FromSeconds(30);
        })
        .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
        {
            MaxConnectionsPerServer = 20 
        });

        services.AddScoped<IDeliveryAPIClient, NovaPoshtaAPIClient>();

        services.AddScoped<ISeeder, OrderStatusSeeder>();
        services.AddScoped<ISeeder, DeliveryProviderSeeder>();
        services.AddScoped<ISeeder, OrderSeeder>();
        services.AddScoped<ISeeder, OrderItemSeeder>();
        services.AddScoped<ISeeder>(sp => new NovaPoshtaSeeder(sp.GetRequiredService<IDeliveryAPIClient>(), sp ));
        services.AddScoped<ISeeder, PickupPointSeeder>();
        services.AddScoped<ISeeder, DeliveryDetailSeeder>();

        ServiceProvider serviceProvider = services.BuildServiceProvider();

        using (IServiceScope scope = serviceProvider.CreateScope())
        {
            IUnitOfWork uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            IEnumerable<ISeeder> seeders = scope.ServiceProvider.GetServices<ISeeder>();

            foreach (ISeeder seeder in seeders)
            {
                Console.WriteLine($"\nSeeding {seeder.GetType().Name}");
                await seeder.SeedAsync(uow);
            }

            Console.WriteLine("All seeding completed!");
        }
    }
}