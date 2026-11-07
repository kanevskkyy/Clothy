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
using Npgsql;

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

        if (string.IsNullOrWhiteSpace(connectionString)) throw new InvalidOperationException("ConnectionStrings__ClothyOrder is not set");

        await WaitForDatabaseAsync(connectionString);

        ServiceCollection services = new ServiceCollection();
        services.AddSingleton<IConnectionFactory>(provider => new ConnectionFactory(connectionString!));
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IOrderRepository, OrderRepository>();
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

        services.AddScoped<ISeeder, OrderSeeder>();
        services.AddScoped<ISeeder, OrderItemSeeder>();
        services.AddScoped<ISeeder>(sp => new NovaPoshtaSeeder(sp.GetRequiredService<IDeliveryAPIClient>(), sp ));
        services.AddScoped<ISeeder, DeliveryProviderSeeder>();
        services.AddScoped<ISeeder, PickupPointSeeder>();
        services.AddScoped<ISeeder, DeliveryDetailSeeder>();

        ServiceProvider serviceProvider = services.BuildServiceProvider();

        using (IServiceScope scope = serviceProvider.CreateScope())
        {
            IUnitOfWork uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            IEnumerable<ISeeder> seeders = scope.ServiceProvider.GetServices<ISeeder>();

            foreach (ISeeder seeder in seeders)
            {
                Console.WriteLine($"Seeding {seeder.GetType().Name}");
                await seeder.SeedAsync(uow);
            }

            Console.WriteLine("All seeding completed!");
        }
    }

    /// <summary>
    /// Same readiness check as catalog-seed: the database accepts connections and the tables from
    /// CreateTablesOrders.sql exist (orders_reservations is the last table the script creates).
    /// </summary>
    private static async Task WaitForDatabaseAsync(string connectionString)
    {
        const int MAX_RETRIES = 30;
        int delayMs = 1000;

        for (int i = 1; i <= MAX_RETRIES; i++)
        {
            try
            {
                Console.WriteLine($"[{i}/{MAX_RETRIES}] Checking database readiness...");

                await using NpgsqlConnection connection = new NpgsqlConnection(connectionString);
                await connection.OpenAsync();

                await using NpgsqlCommand command = new NpgsqlCommand("SELECT 1 FROM orders_reservations LIMIT 1", connection);
                await command.ExecuteScalarAsync();

                Console.WriteLine("Database is ready! Tables exist.");
                return;
            }
            catch (PostgresException ex) when (ex.SqlState == "3D000")
            {
                Console.WriteLine($"Database ClothyOrder is not created yet. Waiting for init scripts... (retry in {delayMs}ms)");
            }
            catch (PostgresException ex) when (ex.SqlState == "42P01")
            {
                Console.WriteLine($"Tables not created yet. Waiting for init scripts... (retry in {delayMs}ms)");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Database not ready: {ex.Message} (retry in {delayMs}ms)");
            }

            if (i < MAX_RETRIES)
            {
                await Task.Delay(delayMs);
                delayMs = Math.Min(delayMs * 2, 10000);
            }
        }

        throw new TimeoutException($"Database was not ready after {MAX_RETRIES} attempts");
    }
}
