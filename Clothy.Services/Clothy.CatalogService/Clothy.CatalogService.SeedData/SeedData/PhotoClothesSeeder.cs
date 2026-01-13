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
            List<Color> colors = await context.Colors.ToListAsync();

            Faker<PhotoClothes> faker = new Faker<PhotoClothes>()
                .RuleFor(p => p.Id, f => Guid.NewGuid())
                .RuleFor(p => p.CreatedAt, f => f.Date.Past(2).ToUniversalTime())
                .RuleFor(p => p.ClotheId, f => f.PickRandom(clotheItems).Id)
                .RuleFor(p => p.ColorId, f => f.PickRandom(colors).Id) 
                .RuleFor(p => p.PhotoURL, f => f.Image.PicsumUrl())
                .RuleFor(p => p.IsMain, f => false);

            List<PhotoClothes> photos = faker.Generate(60);

            var groupedByClotheAndColor = photos.GroupBy(property => new { 
                property.ClotheId, 
                property.ColorId 
            });
            
            foreach (var group in groupedByClotheAndColor)
            {
                var mainPhoto = group.OrderBy(_ => Guid.NewGuid()).First();
                mainPhoto.IsMain = true;
            }

            await context.PhotoClothes.AddRangeAsync(photos);
            await context.SaveChangesAsync();
        }
    }
}
