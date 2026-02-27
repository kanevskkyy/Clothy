using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bogus;
using Clothy.CatalogService.DAL.DB;
using Clothy.CatalogService.Domain.Entities.Catalog;
using Clothy.CatalogService.Domain.Entities.Clothe;
using Clothy.CatalogService.Domain.Entities.Stock;
using Microsoft.EntityFrameworkCore;

namespace Clothy.CatalogService.SeedData.SeedData.Development
{
    public class ClotheItemFakeSeeder : ISeeder
    {
        public async Task SeedAsync(ClothyCatalogDbContext context)
        {
            if (await context.ClotheItems.AnyAsync()) return;

            List<Brand> brands = await context.Brands.ToListAsync();
            List<Collection> collections = await context.Collections.ToListAsync();
            List<ClothingType> clothingTypes = await context.ClothingTypes.ToListAsync();
            List<Color> colors = await context.Colors.ToListAsync();
            List<Size> sizes = await context.Sizes.ToListAsync();
            List<Tag> tags = await context.Tags.ToListAsync();
            List<Material> materials = await context.Materials.ToListAsync();

            Faker faker = new Faker();

            List<ClotheItem> items = new List<ClotheItem>();

            for (int i = 0; i < 40; i++)
            {
                Guid clotheId = Guid.NewGuid();

                List<PhotoClothes> photos = new List<PhotoClothes>();
                for (int j = 0; j < colors.Count; j++)
                {
                    photos.Add(new PhotoClothes
                    {
                        Id = Guid.NewGuid(),
                        ClotheId = clotheId,
                        ColorId = colors[j].Id,
                        PhotoURL = faker.Image.PicsumUrl(),
                        IsMain = j == 0 
                    });
                }

                List<ClothesStock> stocks = new List<ClothesStock>();
                foreach (var size in sizes)
                {
                    foreach (var color in colors)
                    {
                        stocks.Add(new ClothesStock
                        {
                            Id = Guid.NewGuid(),
                            ClotheId = clotheId,
                            SizeId = size.Id,
                            ColorId = color.Id,
                            Quantity = faker.Random.Int(0, 50)
                        });
                    }
                }

                ClotheItem item = new ClotheItem
                {
                    Id = clotheId,
                    CreatedAt = faker.Date.Past(2).ToUniversalTime(),
                    Name = faker.Commerce.ProductName(),
                    Slug = faker.Lorem.Slug(),
                    Description = faker.Lorem.Paragraph(),
                    Price = Math.Round(faker.Random.Decimal(10, 500), 2),
                    OldPrice = faker.Random.Bool(0.3f) ? Math.Round(faker.Random.Decimal(20, 600), 2) : null,
                    BrandId = faker.PickRandom(brands).Id,
                    CollectionId = faker.PickRandom(collections).Id,
                    ClothingTypeId = faker.PickRandom(clothingTypes).Id,
                    Gender = faker.PickRandom<Gender>(),

                    ClothePopularities = new List<ClothePopularity>
                    {
                        new ClothePopularity
                        {
                            Id = Guid.NewGuid(),
                            ClotheId = clotheId,
                            SoldCount = faker.Random.Int(0, 1000)
                        }
                    },
                    Photos = photos,
                    Stocks = stocks,
                    ClotheTags = new List<ClotheTag>
                    {
                        new ClotheTag
                        {
                            TagId = tags[0].Id
                        },
                        new ClotheTag
                        {
                            TagId = tags[1].Id
                        }
                    },
                    ClotheMaterials = new List<ClotheMaterial>
                    {
                        new ClotheMaterial
                        {
                            MaterialId = materials[0].Id,
                            Percentage = 70
                        },
                        new ClotheMaterial
                        {
                            MaterialId = materials[1].Id,
                            Percentage = 30
                        }
                    }
                };

                items.Add(item);
            }

            await context.ClotheItems.AddRangeAsync(items);
            await context.SaveChangesAsync();
        }
    }
}