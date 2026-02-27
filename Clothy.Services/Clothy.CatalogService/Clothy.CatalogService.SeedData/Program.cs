using Clothy.CatalogService.DAL.DB;
using Clothy.CatalogService.SeedData.SeedData;
using Clothy.CatalogService.SeedData.SeedData.Always;
using Clothy.CatalogService.SeedData.SeedData.Development;
using Clothy.CatalogService.SeedData.SeedData.Production;
using DotNetEnv;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace Clothy.CatalogService.SeedData
{
    public class Program
    {
        public static async Task Main()
        {
            Env.Load();

            string? seedMode = Environment.GetEnvironmentVariable("SEED__MODE");

            if (seedMode != null && seedMode != "Real" && seedMode != "Fake") throw new InvalidOperationException($"Invalid SEED__MODE value: '{seedMode}'. Expected values are 'Real' or 'Fake'");

            IConfigurationRoot builder = new ConfigurationBuilder()
                .AddEnvironmentVariables()
                .Build();

            string? connectionString = builder.GetConnectionString("ClothyCatalogDb");
            Console.WriteLine($"Using connection string: {connectionString}");

            DbContextOptions<ClothyCatalogDbContext> options = new DbContextOptionsBuilder<ClothyCatalogDbContext>()
                .UseNpgsql(connectionString)
                .Options;

            using ClothyCatalogDbContext context = new ClothyCatalogDbContext(options);

            await WaitForDatabaseAsync(context);

            List<ISeeder> seeders = new List<ISeeder>
            {
                new SizeSeeder(),
                new MaterialSeeder(),
                new ColorSeeder(),
                new ClothingTypeSeeder(),
            };

            if (seedMode == "Real")
            {
                seeders.Add(new BrandSeeder());
                seeders.Add(new CollectionSeeder());
                seeders.Add(new TagSeeder());
                seeders.Add(new ManClotheSeeder());
                seeders.Add(new WomanClotheSeeder());
            }
            else
            {
                seeders.Add(new BrandFakeSeeder());
                seeders.Add(new CollectionFakeSeeder());
                seeders.Add(new TagFakeSeeder());
                seeders.Add(new ClotheItemFakeSeeder());
            }

            Console.WriteLine("Start seeding data!");

            foreach (ISeeder seeder in seeders)
            {
                await seeder.SeedAsync(context);
            }

            Console.WriteLine("Seeding completed!");
        }

        private static async Task WaitForDatabaseAsync(ClothyCatalogDbContext context)
        {
            const int MAX_RETRIES = 30;
            int delayMs = 1000;

            for (int i = 1; i <= MAX_RETRIES; i++)
            {
                try
                {
                    Console.WriteLine($"[{i}/{MAX_RETRIES}] Checking database readiness...");

                    bool canConnect = await context.Database.CanConnectAsync();
                    if (!canConnect) throw new Exception("Cannot connect to database");

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

                if (i < MAX_RETRIES)
                {
                    await Task.Delay(delayMs);
                    delayMs = Math.Min(delayMs * 2, 10000);
                }
            }

            throw new TimeoutException($"Database was not ready after {MAX_RETRIES} attempts (waited ~{MAX_RETRIES * 2}s)");
        }
    }
}