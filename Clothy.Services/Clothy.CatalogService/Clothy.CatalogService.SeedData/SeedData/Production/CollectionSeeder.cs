using Clothy.CatalogService.DAL.DB;
using Clothy.CatalogService.Domain.Entities.Catalog;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clothy.CatalogService.SeedData.SeedData.Production
{
    public class CollectionSeeder : ISeeder
    {
        public async Task SeedAsync(ClothyCatalogDbContext context)
        {
            if (await context.Collections.AnyAsync()) return;

            List<Collection> collections = new List<Collection>
            {
                new Collection
                {
                    Id = Guid.NewGuid(),
                    Name = "Spring 2025",
                    Slug = "spring-2025",
                    CreatedAt = DateTime.UtcNow.ToUniversalTime()
                },
                new Collection
                {
                    Id = Guid.NewGuid(),
                    Name = "Summer 2025",
                    Slug = "summer-2025",
                    CreatedAt = DateTime.UtcNow.ToUniversalTime()
                },
                new Collection
                {
                    Id = Guid.NewGuid(),
                    Name = "Autumn 2025",
                    Slug = "autumn-2025",
                    CreatedAt = DateTime.UtcNow.ToUniversalTime()
                },
                new Collection
                {
                    Id = Guid.NewGuid(),
                    Name = "Winter 2025",
                    Slug = "winter-2025",
                    CreatedAt = DateTime.UtcNow.ToUniversalTime()
                },
                new Collection
                {
                    Id = Guid.NewGuid(),
                    Name = "Spring 2026",
                    Slug = "spring-2026",
                    CreatedAt = DateTime.UtcNow.ToUniversalTime()
                },
                new Collection
                {
                    Id = Guid.NewGuid(),
                    Name = "Summer 2026",
                    Slug = "summer-2026",
                    CreatedAt = DateTime.UtcNow.ToUniversalTime()
                }
            };

            await context.Collections.AddRangeAsync(collections);
            await context.SaveChangesAsync();
        }
    }
}