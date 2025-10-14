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
    public class ClotheMaterialSeeder : ISeeder
    {
        public async Task SeedAsync(ClothyCatalogDbContext context)
        {
            if (await context.ClotheMaterials.AnyAsync()) return;

            List<ClotheItem> clotheItems = await context.ClotheItems.ToListAsync();
            List<Material> materials = await context.Materials.ToListAsync();
            Faker faker = new Faker();

            List<ClotheMaterial> clotheMaterials = new List<ClotheMaterial>();

            foreach (ClotheItem clothe in clotheItems)
            {
                List<Material> randomMaterials = faker.PickRandom(materials, faker.Random.Int(1, 3)).Distinct().ToList();

                foreach (Material material in randomMaterials)
                {
                    ClotheMaterial clotheMaterial = new ClotheMaterial
                    {
                        ClotheId = clothe.Id,
                        MaterialId = material.Id,
                        Percentage = Math.Round(faker.Random.Decimal(10, 90), 2)
                    };

                    clotheMaterials.Add(clotheMaterial);
                }
            }

            await context.ClotheMaterials.AddRangeAsync(clotheMaterials);
            await context.SaveChangesAsync();
        }
    }
}
