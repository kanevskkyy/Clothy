using Bogus;
using Clothy.CatalogService.DAL.DB;
using Clothy.CatalogService.Domain.Entities.Catalog;
using Microsoft.EntityFrameworkCore;

namespace Clothy.CatalogService.SeedData.SeedData.Development
{
    public class CollectionFakeSeeder : ISeeder
    {
        public async Task SeedAsync(ClothyCatalogDbContext context)
        {
            if (await context.Collections.AnyAsync()) return;

            Faker<Collection> faker = new Faker<Collection>()
                .RuleFor(c => c.Id, _ => Guid.NewGuid())
                .RuleFor(c => c.CreatedAt, f => f.Date.Past(3).ToUniversalTime())
                .RuleFor(c => c.Name, f => $"{f.Company.CompanyName()} x {f.Company.CompanySuffix()}")
                .RuleFor(c => c.Slug, (f, c) =>
                    c.Name
                        .ToLower()
                        .Replace("&", "and")
                        .Replace(" ", "-")
                        .Replace(".", "")
                        .Replace(",", "")
                );

            List<Collection> collections = faker.Generate(15);

            await context.Collections.AddRangeAsync(collections);
            await context.SaveChangesAsync();
        }
    }
}