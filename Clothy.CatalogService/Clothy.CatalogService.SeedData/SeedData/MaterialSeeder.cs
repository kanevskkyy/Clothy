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
    public class MaterialSeeder : ISeeder
    {
        public async Task SeedAsync(ClothyCatalogDbContext context)
        {
            if (await context.Materials.AnyAsync()) return;

            List<string> materialNames = new List<string>()
            {
                "Cotton",
                "Wool",
                "Silk",
                "Cashmere",
                "Viscose",
                "Acrylic",
                "Spandex",
                "Satin",
                "Fleece"
            };

            List<Material> materials = new List<Material>();
            Faker faker = new Faker();

            foreach (string materialName in materialNames)
            {
                Material material = new Material
                {
                    Id = Guid.NewGuid(),
                    CreatedAt = faker.Date.Past(2).ToUniversalTime(),
                    Name = materialName,
                    Slug = materialName.ToLower()
                };

                materials.Add(material);
            }

            await context.Materials.AddRangeAsync(materials);
            await context.SaveChangesAsync();
        }
    }
}
