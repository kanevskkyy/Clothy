using Bogus;
using Clothy.CatalogService.DAL.DB;
using Clothy.CatalogService.Domain.Entities.Catalog;
using Microsoft.EntityFrameworkCore;

namespace Clothy.CatalogService.SeedData.SeedData.Development
{
    public class TagFakeSeeder : ISeeder
    {
        public async Task SeedAsync(ClothyCatalogDbContext context)
        {
            if (await context.Tags.AnyAsync()) return;

            Faker<Tag> faker = new Faker<Tag>()
                .RuleFor(t => t.Id, _ => Guid.NewGuid())
                .RuleFor(t => t.CreatedAt, f => f.Date.Past(3).ToUniversalTime())
                .RuleFor(t => t.Name, f => f.Commerce.Categories(1)[0]) 
                .RuleFor(t => t.Slug, (f, t) =>
                    t.Name
                        .ToLower()
                        .Replace("&", "and")
                        .Replace(" ", "-")
                        .Replace(".", "")
                        .Replace(",", "")
                );

            List<Tag> tags = faker.Generate(15);

            await context.Tags.AddRangeAsync(tags);
            await context.SaveChangesAsync();
        }
    }
}