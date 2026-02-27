using Bogus;
using Clothy.CatalogService.DAL.DB;
using Clothy.CatalogService.Domain.Entities.Catalog;
using Microsoft.EntityFrameworkCore;

namespace Clothy.CatalogService.SeedData.SeedData.Development
{
    public class BrandFakeSeeder : ISeeder
    {
        public async Task SeedAsync(ClothyCatalogDbContext context)
        {
            if (await context.Brands.AnyAsync()) return;

            Faker<Brand> faker = new Faker<Brand>()
                .RuleFor(b => b.Id, _ => Guid.NewGuid())
                .RuleFor(b => b.CreatedAt, f => f.Date.Past(3).ToUniversalTime())
                .RuleFor(b => b.Name, f => f.Company.CompanyName())
                .RuleFor(b => b.Slug, (f, b) =>
                    b.Name
                     .ToLower()
                     .Replace("&", "and")
                     .Replace(".", "")
                     .Replace(",", "")
                     .Replace(" ", "-")
                );

            var brands = faker.Generate(15);

            await context.Brands.AddRangeAsync(brands);
            await context.SaveChangesAsync();
        }
    }
}