using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bogus;
using Clothy.CatalogService.DAL.DB;
using Clothy.CatalogService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Clothy.CatalogService.SeedData.SeedData
{
    public class ClothingTypeSeeder : ISeeder
    {
        public async Task SeedAsync(ClothyCatalogDbContext context)
        {
            if (await context.ClothingTypes.AnyAsync()) return;

            List<string> clothingTypeNames = new List<string>()
            {
                "T-Shirt",
                "Zip Hoodie",
                "Sweatshirt",
                "Long Sleeve Shirt",
                "Jacket",
                "Coat",
                "Polo Shirt",
                "Tank Top",
                "Hoodie",
                "Cardigan",
                "Blazer",
                "Sweater",
                "Shirt"
            };

            List<ClothingType> clothingTypes = new List<ClothingType>();
            Faker faker = new Faker();

            foreach (string typeName in clothingTypeNames)
            {
                ClothingType clothingType = new ClothingType
                {
                    Id = Guid.NewGuid(),
                    CreatedAt = faker.Date.Past(2).ToUniversalTime(),
                    Name = typeName,
                    Slug = typeName.ToLower().Replace(" ", "-")
                };

                clothingTypes.Add(clothingType);
            }

            await context.ClothingTypes.AddRangeAsync(clothingTypes);
            await context.SaveChangesAsync();
        }
    }
}
