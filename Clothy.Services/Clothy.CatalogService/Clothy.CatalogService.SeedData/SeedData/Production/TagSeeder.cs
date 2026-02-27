using Clothy.CatalogService.DAL.DB;
using Clothy.CatalogService.Domain.Entities.Catalog;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Clothy.CatalogService.SeedData.SeedData.Production
{
    public class TagSeeder : ISeeder
    {
        public async Task SeedAsync(ClothyCatalogDbContext context)
        {
            if (await context.Tags.AnyAsync()) return;

            List<Tag> tags = new List<Tag>
            {
                new Tag
                {
                    Id = Guid.NewGuid(),
                    Name = "Sport",
                    Slug = "sport",
                    CreatedAt = DateTime.UtcNow.ToUniversalTime()
                },
                new Tag
                {
                    Id = Guid.NewGuid(),
                    Name = "Casual",
                    Slug = "casual",
                    CreatedAt = DateTime.UtcNow.ToUniversalTime()
                },
                new Tag
                {
                    Id = Guid.NewGuid(),
                    Name = "Streetwear",
                    Slug = "streetwear",
                    CreatedAt = DateTime.UtcNow.ToUniversalTime()
                },
                new Tag
                {
                    Id = Guid.NewGuid(),
                    Name = "Elegant",
                    Slug = "elegant",
                    CreatedAt = DateTime.UtcNow.ToUniversalTime()
                },
                new Tag
                {
                    Id = Guid.NewGuid(),
                    Name = "Outdoor",
                    Slug = "outdoor",
                    CreatedAt = DateTime.UtcNow.ToUniversalTime()
                },
                new Tag
                {
                    Id = Guid.NewGuid(),
                    Name = "Travel",
                    Slug = "travel",
                    CreatedAt = DateTime.UtcNow.ToUniversalTime()
                },
                new Tag
                {
                    Id = Guid.NewGuid(),
                    Name = "Comfort",
                    Slug = "comfort",
                    CreatedAt = DateTime.UtcNow.ToUniversalTime()
                },
                new Tag
                {
                    Id = Guid.NewGuid(),
                    Name = "Summer",
                    Slug = "summer",
                    CreatedAt = DateTime.UtcNow.ToUniversalTime()
                },
                new Tag
                {
                    Id = Guid.NewGuid(),
                    Name = "Winter",
                    Slug = "winter",
                    CreatedAt = DateTime.UtcNow.ToUniversalTime()
                },
                new Tag
                {
                    Id = Guid.NewGuid(),
                    Name = "Autumn",
                    Slug = "autumn",
                    CreatedAt = DateTime.UtcNow.ToUniversalTime()
                },
                new Tag
                {
                    Id = Guid.NewGuid(),
                    Name = "Spring",
                    Slug = "spring",
                    CreatedAt = DateTime.UtcNow.ToUniversalTime()
                },
                new Tag
                {
                    Id = Guid.NewGuid(),
                    Name = "Limited Edition",
                    Slug = "limited-edition",
                    CreatedAt = DateTime.UtcNow.ToUniversalTime()
                },
                new Tag
                {
                    Id = Guid.NewGuid(),
                    Name = "New Arrival",
                    Slug = "new-arrival",
                    CreatedAt = DateTime.UtcNow.ToUniversalTime()
                },
                new Tag
                {
                    Id = Guid.NewGuid(),
                    Name = "Formal",
                    Slug = "formal",
                    CreatedAt = DateTime.UtcNow.ToUniversalTime()
                },
                new Tag
                {
                    Id = Guid.NewGuid(),
                    Name = "Business",
                    Slug = "business",
                    CreatedAt = DateTime.UtcNow.ToUniversalTime()
                }
            };

            await context.Tags.AddRangeAsync(tags);
            await context.SaveChangesAsync();
        }
    }
}