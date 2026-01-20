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

            List<(string Name, string slug, string Hex)> colorsData = new List<(string Name, string slug, string Hex)>
            {
                ("Red", "red", "#FF5733"),
                ("Green", "green", "#33FF57"),
                ("Blue", "blue", "#3357FF"),
                ("Yellow", "yellow", "#F1C40F"),
                ("Purple", "purple", "#9B59B6"),
                ("Turquoise", "turquoise", "#1ABC9C"),
                ("Orange", "orange", "#E67E22"),
                ("Silver", "silver", "#BDC3C7"),
                ("Dark Blue", "dark-blue", "#2C3E50"),
                ("White", "white", "#ECF0F1")
            };

            Faker faker = new Faker();
            List<Color> colors = new();

            foreach ((string name, string slug, string hex) in colorsData)
            {
                colors.Add(new Color
                {
                    Id = Guid.NewGuid(),
                    CreatedAt = faker.Date.Past(2).ToUniversalTime(),
                    Name = name,
                    HexCode = hex,
                    Slug = slug
                });
            }

            await context.Colors.AddRangeAsync(colors);
            await context.SaveChangesAsync();
        }
    }
}
