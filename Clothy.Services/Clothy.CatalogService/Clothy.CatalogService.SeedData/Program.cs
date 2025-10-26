using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.CatalogService.DAL.DB;
using Clothy.CatalogService.SeedData.SeedData;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

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

            Console.WriteLine("Applying migrations...");
            await context.Database.MigrateAsync();
            Console.WriteLine("Migrations applied!");

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
    }
}
