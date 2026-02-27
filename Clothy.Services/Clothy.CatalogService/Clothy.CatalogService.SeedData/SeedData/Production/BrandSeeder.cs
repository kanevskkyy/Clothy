using Clothy.CatalogService.DAL.DB;
using Clothy.CatalogService.Domain.Entities.Catalog;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Clothy.CatalogService.SeedData.SeedData.Production
{
    public class BrandSeeder : ISeeder
    {
        public async Task SeedAsync(ClothyCatalogDbContext context)
        {
            if (await context.Brands.AnyAsync()) return;

            List<Brand> brands = new List<Brand>
            {
                new Brand
                {
                    Id = Guid.NewGuid(),
                    Name = "Nike",
                    Slug = "nike",
                    CreatedAt = DateTime.UtcNow.ToUniversalTime()
                },
                new Brand
                {
                    Id = Guid.NewGuid(),
                    Name = "Adidas",
                    Slug = "adidas",
                    CreatedAt = DateTime.UtcNow.ToUniversalTime()
                },
                new Brand
                {
                    Id = Guid.NewGuid(),
                    Name = "Off-White",
                    Slug = "off-white",
                    CreatedAt = DateTime.UtcNow.ToUniversalTime()
                },
                new Brand
                {
                    Id = Guid.NewGuid(),
                    Name = "Balenciaga",
                    Slug = "balenciaga",
                    CreatedAt = DateTime.UtcNow.ToUniversalTime()
                },
                new Brand
                {
                    Id = Guid.NewGuid(),
                    Name = "Stone Island",
                    Slug = "stone-island",
                    CreatedAt = DateTime.UtcNow.ToUniversalTime()
                },
                new Brand
                {
                    Id = Guid.NewGuid(),
                    Name = "Palm Angels",
                    Slug = "palm-angels",
                    CreatedAt = DateTime.UtcNow.ToUniversalTime()
                },
                new Brand
                {
                    Id = Guid.NewGuid(),
                    Name = "Gucci",
                    Slug = "gucci",
                    CreatedAt = DateTime.UtcNow.ToUniversalTime()
                },
                new Brand
                {
                    Id = Guid.NewGuid(),
                    Name = "Louis Vuitton",
                    Slug = "louis-vuitton",
                    CreatedAt = DateTime.UtcNow.ToUniversalTime()
                },
                new Brand
                {
                    Id = Guid.NewGuid(),
                    Name = "BAPE",
                    Slug = "bape",
                    CreatedAt = DateTime.UtcNow.ToUniversalTime()
                },
                new Brand
                {
                    Id = Guid.NewGuid(),
                    Name = "Puma",
                    Slug = "puma",
                    CreatedAt = DateTime.UtcNow.ToUniversalTime()
                },
                new Brand
                {
                    Id = Guid.NewGuid(),
                    Name = "Reebok",
                    Slug = "reebok",
                    CreatedAt = DateTime.UtcNow.ToUniversalTime()
                },
                new Brand
                {
                    Id = Guid.NewGuid(),
                    Name = "Under Armour",
                    Slug = "under-armour",
                    CreatedAt = DateTime.UtcNow.ToUniversalTime()
                },
                new Brand
                {
                    Id = Guid.NewGuid(),
                    Name = "New Balance",
                    Slug = "new-balance",
                    CreatedAt = DateTime.UtcNow.ToUniversalTime()
                },
                new Brand
                {
                    Id = Guid.NewGuid(),
                    Name = "Supreme",
                    Slug = "supreme",
                    CreatedAt = DateTime.UtcNow.ToUniversalTime()
                },
                new Brand
                {
                    Id = Guid.NewGuid(),
                    Name = "Fear of God",
                    Slug = "fear-of-god",
                    CreatedAt = DateTime.UtcNow.ToUniversalTime()
                }
            };

            await context.Brands.AddRangeAsync(brands);
            await context.SaveChangesAsync();
        }
    }
}