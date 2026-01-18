using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bogus;
using Clothy.CatalogService.DAL.DB;
using Clothy.CatalogService.Domain.Entities.Catalog;
using Microsoft.EntityFrameworkCore;

namespace Clothy.CatalogService.SeedData.SeedData
{
    public class ColorSeeder : ISeeder
    {
        public async Task SeedAsync(ClothyCatalogDbContext context)
        {
            if (await context.Colors.AnyAsync()) return;

            List<(string Name, string Hex)> colorsData = new List<(string Name, string Hex)>
            {
                ("Red", "#FF5733"),
                ("Green", "#33FF57"),
                ("Blue", "#3357FF"),
                ("Yellow", "#F1C40F"),
                ("Purple", "#9B59B6"),
                ("Turquoise", "#1ABC9C"),
                ("Orange", "#E67E22"),
                ("Silver", "#BDC3C7"),
                ("Dark Blue", "#2C3E50"),
                ("White", "#ECF0F1")
            };

            Faker faker = new Faker();
            List<Color> colors = new();

            foreach ((string name, string hex) in colorsData)
            {
                colors.Add(new Color
                {
                    Id = Guid.NewGuid(),
                    CreatedAt = faker.Date.Past(2).ToUniversalTime(),
                    Name = name,
                    HexCode = hex
                });
            }

            await context.Colors.AddRangeAsync(colors);
            await context.SaveChangesAsync();
        }
    }
}
