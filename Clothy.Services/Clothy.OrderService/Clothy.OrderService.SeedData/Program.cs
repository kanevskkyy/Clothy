using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Clothy.OrderService.DAL.ConnectionFactory;
using Clothy.OrderService.DAL.UOW;
using Clothy.OrderService.SeedData;
using Clothy.OrderService.SeedData.Seeders;
using Clothy.OrderService.DAL.Repositories;
using Clothy.OrderService.DAL.Interfaces;
using Microsoft.Extensions.Configuration;
using Dapper;

class Program
{
    public static async Task Main()
    {
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
        services.AddScoped<ICityRepository, CityRepository>();
        services.AddScoped<IOrderItemRepository, OrderItemRepository>();
        services.AddScoped<IDeliveryDetailRepository, DeliveryDetailRepository>();
        services.AddScoped<IRegionRepository, RegionRepository>();

        services.AddScoped<ISeeder, OrderStatusSeeder>();
        services.AddScoped<ISeeder, DeliveryProviderSeeder>();
        services.AddScoped<ISeeder, CitySeeder>();
        services.AddScoped<ISeeder, OrderSeeder>();
        services.AddScoped<ISeeder, OrderItemSeeder>();
        services.AddScoped<ISeeder, DeliveryDetailSeeder>();
        services.AddScoped<ISeeder, RegionSeeder>();

        ServiceProvider serviceProvider = services.BuildServiceProvider();

        using (IServiceScope scope = serviceProvider.CreateScope())
        {
            IUnitOfWork uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            IEnumerable<ISeeder> seeders = scope.ServiceProvider.GetServices<ISeeder>();

            foreach (ISeeder seeder in seeders)
            {
                Console.WriteLine($"Seeding {seeder.GetType().Name}...");
                await seeder.SeedAsync(uow);
            }

            Console.WriteLine("Seeding completed!");
        }
    }
}