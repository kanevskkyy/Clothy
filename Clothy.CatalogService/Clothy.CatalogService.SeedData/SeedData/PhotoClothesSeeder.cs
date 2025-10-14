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
    public class PhotoClothesSeeder : ISeeder
    {
        public async Task SeedAsync(ClothyCatalogDbContext context)
        {
            if (await context.PhotoClothes.AnyAsync()) return;

            List<ClotheItem> clotheItems = await context.ClotheItems.ToListAsync();

            Faker<PhotoClothes> faker = new Faker<PhotoClothes>()
                .RuleFor(p => p.Id, fakeData => Guid.NewGuid())
                .RuleFor(p => p.CreatedAt, fakeData => fakeData.Date.Past(2).ToUniversalTime())
                .RuleFor(p => p.ClotheId, fakeData => fakeData.PickRandom(clotheItems).Id)
                .RuleFor(p => p.PhotoURL, fakeData => fakeData.Image.PicsumUrl());

            List<PhotoClothes> photos = faker.Generate(60);

            await context.PhotoClothes.AddRangeAsync(photos);
            await context.SaveChangesAsync();
        }
    }
}
