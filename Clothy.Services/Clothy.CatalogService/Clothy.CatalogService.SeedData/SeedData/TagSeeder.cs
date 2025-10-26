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
    public class TagSeeder : ISeeder
    {
        public async Task SeedAsync(ClothyCatalogDbContext context)
        {
            if (await context.Tags.AnyAsync()) return;

            List<string> tagNames = new List<string>()
            {
                "Summer",
                "Winter",
                "Autumn",
                "Spring",
                "Casual",
                "Sport",
                "Streetwear",
                "Elegant",
                "Outdoor",
                "Travel",
                "Comfort",
                "New Arrival",
                "Limited Edition",
                "Unisex"
            };

            List<Tag> tags = new List<Tag>();
            Faker faker = new Faker();

            foreach (string tagName in tagNames)
            {
                Tag tag = new Tag
                {
                    Id = Guid.NewGuid(),
                    CreatedAt = faker.Date.Past(2).ToUniversalTime(),
                    Name = tagName
                };

                tags.Add(tag);
            }

            await context.Tags.AddRangeAsync(tags);
            await context.SaveChangesAsync();
        }
    }
}
