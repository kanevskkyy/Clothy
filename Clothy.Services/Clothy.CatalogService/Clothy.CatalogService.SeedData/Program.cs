using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.CatalogService.DAL.DB;
using Clothy.CatalogService.SeedData.SeedData;
using Microsoft.EntityFrameworkCore;

namespace Clothy.CatalogService.SeedData
{
    public class Program
    {
        public static async Task Main()
        {
            const string CONNECTION_STRING = "Host=localhost;Port=5432;Database=ClothyCatalog;Username=postgres;Password=postgres";
            var options = new DbContextOptionsBuilder<ClothyCatalogDbContext>()
                .UseNpgsql(CONNECTION_STRING)
                .Options;

            using ClothyCatalogDbContext context = new ClothyCatalogDbContext(options);

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
