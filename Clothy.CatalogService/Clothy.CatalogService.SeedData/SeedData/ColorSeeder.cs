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
    public class ColorSeeder : ISeeder
    {
        public async Task SeedAsync(ClothyCatalogDbContext context)
        {
            if (await context.Colors.AnyAsync()) return;

            List<string> colorHexes = new List<string>()
            {
                "#FF5733",
                "#33FF57",
                "#3357FF",
                "#F1C40F",
                "#9B59B6",
                "#1ABC9C",
                "#E67E22",
                "#BDC3C7",
                "#2C3E50",
                "#ECF0F1"
            };

            List<Color> colors = new List<Color>();
            Faker faker = new Faker();

            foreach (string hex in colorHexes)
            {
                Color color = new Color
                {
                    Id = Guid.NewGuid(),
                    CreatedAt = faker.Date.Past(2).ToUniversalTime(),
                    HexCode = hex
                };

                colors.Add(color);
            }

            await context.Colors.AddRangeAsync(colors);
            await context.SaveChangesAsync();
        }
    }
}
