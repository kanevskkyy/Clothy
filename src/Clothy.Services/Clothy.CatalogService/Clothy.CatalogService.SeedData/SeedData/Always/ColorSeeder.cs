using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bogus;
using Clothy.CatalogService.DAL.DB;
using Clothy.CatalogService.Domain.Entities.Catalog;
using Microsoft.EntityFrameworkCore;

namespace Clothy.CatalogService.SeedData.SeedData.Always
{
    public class ColorSeeder : ISeeder
    {
        public async Task SeedAsync(ClothyCatalogDbContext context)
        {
            if (await context.Colors.AnyAsync()) return;

            List<(string Name, string Slug, string Hex)> colorsData = new List<(string Name, string Slug, string Hex)>
            {
                ("Black", "black", "#000000"),
                ("White", "white", "#FFFFFF"),
                ("Red", "red", "#FF0000"),
                ("Blue", "blue", "#233656"),
                ("Green", "green", "#105A33"),
                ("Yellow", "yellow", "#FFFF00"),
                ("Gray", "gray", "#808080"),
                ("Brown", "brown", "#8B4513"),
                ("Beige", "beige", "#F5F5DC"),
                ("Navy", "navy", "#000080"),
                ("Soft Peach", "soft-peach", "#E4B8A3"),
                ("Warm Taupe", "warm-taupe", "#A77F6B"),
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
