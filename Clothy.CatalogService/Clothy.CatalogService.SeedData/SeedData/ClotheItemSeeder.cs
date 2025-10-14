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
    public class ClotheItemSeeder : ISeeder
    {
        public async Task SeedAsync(ClothyCatalogDbContext context)
        {
            if (await context.ClotheItems.AnyAsync()) return;

            List<Brand> brands = await context.Brands.ToListAsync();
            List<Collection> collections = await context.Collections.ToListAsync();
            List<ClothingType> clothingTypes = await context.ClothingTypes.ToListAsync();

            Faker<ClotheItem> faker = new Faker<ClotheItem>()
                .RuleFor(p => p.Id, fakeData => Guid.NewGuid())
                .RuleFor(p => p.CreatedAt, fakeData => fakeData.Date.Past(2).ToUniversalTime())
                .RuleFor(p => p.Name, fakeData => fakeData.Commerce.ProductName())
                .RuleFor(p => p.Slug, fakeData => fakeData.Lorem.Slug())
                .RuleFor(p => p.Description, fakeData => fakeData.Lorem.Paragraph())
                .RuleFor(p => p.MainPhotoURL, fakeData => fakeData.Image.PicsumUrl())
                .RuleFor(p => p.Price, fakeData => Math.Round(fakeData.Random.Decimal(10, 500), 2))
                .RuleFor(p => p.BrandId, fakeData => fakeData.PickRandom(brands).Id)
                .RuleFor(p => p.CollectionId, fakeData => fakeData.PickRandom(collections).Id)
                .RuleFor(p => p.ClothingTypeId, fakeData => fakeData.PickRandom(clothingTypes).Id);

            List<ClotheItem> clotheItems = faker.Generate(40);
            
            await context.ClotheItems.AddRangeAsync(clotheItems);
            await context.SaveChangesAsync();
        }
    }
}
