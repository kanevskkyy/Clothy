using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.CatalogService.DAL.DB;
using Clothy.CatalogService.SeedData.SeedData;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace Clothy.CatalogService.SeedData
{
    public class Program
    {
        public static async Task Main()
        {
            var builder = new ConfigurationBuilder()
                    .AddEnvironmentVariables()  
                    .Build();

            string? connectionString = builder.GetConnectionString("ClothyCatalogDb");
            Console.WriteLine($"Using connection string: {connectionString}");

            var options = new DbContextOptionsBuilder<ClothyCatalogDbContext>()
                .UseNpgsql(connectionString)
                .Options;

            using ClothyCatalogDbContext context = new ClothyCatalogDbContext(options);

            await WaitForDatabaseAsync(context);

            ISeeder[] seeders = new ISeeder[]
            {
                new BrandSeeder(),
                new TagSeeder(),
                new SizeSeeder(),
                new MaterialSeeder(),
                new ColorSeeder(), 
                new CollectionSeeder(),
                new ClothingTypeSeeder(),
                new ClotheItemSeeder(),
                new ClotheMaterialSeeder(),
                new ClothesStockSeeder(),
                new ClotheTagSeeder(),
                new PhotoClothesSeeder(),
            };

            Console.WriteLine("Start seeding data!");
            
            foreach(ISeeder seeder in seeders)
            {
                await seeder.SeedAsync(context);
            }
            
            Console.WriteLine("Seeding completed!");
        }

        private static async Task WaitForDatabaseAsync(ClothyCatalogDbContext context)
        {
            const int maxRetries = 30;
            int delayMs = 1000;

            for (int i = 1; i <= maxRetries; i++)
            {
                try
                {
                    Console.WriteLine($"[{i}/{maxRetries}] Checking database readiness...");
                    
                    var canConnect = await context.Database.CanConnectAsync();
                    if (!canConnect)
                    {
                        throw new Exception("Cannot connect to database");
                    }
                    await context.Database.ExecuteSqlRawAsync("SELECT 1 FROM brands LIMIT 1");
                    
                    Console.WriteLine("Database is ready! Tables exist, migrations completed.");
                    return;
                }
                catch (PostgresException ex) when (ex.SqlState == "42P01") 
                {
                    Console.WriteLine($"Tables not created yet. Waiting for migrations... (retry in {delayMs}ms)");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Database not ready: {ex.Message} (retry in {delayMs}ms)");
                }

                if (i < maxRetries)
                {
                    await Task.Delay(delayMs);
                    delayMs = Math.Min(delayMs * 2, 10000);
                }
            }

            throw new TimeoutException($"Database was not ready after {maxRetries} attempts (waited ~{maxRetries * 2}s)");
        }
    }
}