using Clothy.CatalogService.DAL.DB;
using Clothy.CatalogService.Domain.Entities.Catalog;
using Clothy.CatalogService.Domain.Entities.Clothe;
using Clothy.CatalogService.Domain.Entities.Stock;
using Clothy.CatalogService.SeedData.SeedData.Production.Helpers;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Clothy.CatalogService.SeedData.SeedData.Production
{
    public class ManClotheSeeder : ISeeder
    {
        private static Random random = new Random();

        private static Dictionary<string, int> manClothePopularity = new Dictionary<string, int>()
        {
            {
                "Stone Island Garment Dyed Crewneck Sweatshirt", 1000   
            },
            {
                "Balenciaga BB Paris Icon Oversized T-Shirt", 999
            },
            {
                "Bookish Skate Sweatshirt", 998
            },
            {
                "Fear of God Eternal Fleece Hoodie", 997
            }
        };

        public async Task SeedAsync(ClothyCatalogDbContext context)
        {
            if (await context.ClotheItems.AnyAsync(p => p.Gender == Gender.Male)) return;

            Dictionary<string, Tag> tags = await context.Tags.ToDictionaryAsync(t => t.Name);
            Dictionary<string, Material> materials = await context.Materials.ToDictionaryAsync(m => m.Name);
            Dictionary<string, Brand> brands = await context.Brands.ToDictionaryAsync(b => b.Name);
            Dictionary<string, ClothingType> types = await context.ClothingTypes.ToDictionaryAsync(t => t.Name);
            Dictionary<string, Collection> collections = await context.Collections.ToDictionaryAsync(c => c.Name);
            Dictionary<string, Color> colors = await context.Colors.ToDictionaryAsync(c => c.Name);
            List<Size> sizes = await context.Sizes.ToListAsync();

            string json = await File.ReadAllTextAsync("SeedData/Production/Data/man_clothes.json");
            List<ClotheItemSeedDto> dtos = JsonSerializer.Deserialize<List<ClotheItemSeedDto>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            })!;

            List<ClotheItem> items = dtos.Select(d =>
            {
                Guid clotheId = Guid.NewGuid();

                List<Color> itemColors = d.Photos
                    .Select(p => p.Color)
                    .Distinct()
                    .Select(name => colors[name])
                    .ToList();

                List<ClothesStock> stocks = new List<ClothesStock>();

                foreach (Color color in itemColors)
                {
                    int zeroSizeIndex = random.Next(sizes.Count);

                    for (int i = 0; i < sizes.Count; i++)
                    {
                        stocks.Add(new ClothesStock
                        {
                            Id = Guid.NewGuid(),
                            ClotheId = clotheId,
                            SizeId = sizes[i].Id,
                            ColorId = color.Id,
                            Quantity = i == zeroSizeIndex ? 0 : random.Next(5, 50)
                        });
                    }
                }

                return new ClotheItem
                {
                    Id = clotheId,
                    Name = d.Name,
                    Slug = d.Slug,
                    Description = d.Description,
                    Price = d.Price,
                    OldPrice = d.OldPrice,
                    Gender = Gender.Male,
                    Brand = brands[d.Brand],
                    ClothyType = types[d.Type],
                    Collection = collections[d.Collection],
                    ClotheTags = d.Tags
                        .Select(t => new ClotheTag { 
                            Tag = tags[t] 
                        }).ToList(),
                    ClotheMaterials = d.Materials
                        .Select(m => new ClotheMaterial { 
                            Material = materials[m.Name], 
                            Percentage = m.Percentage }
                        ).ToList(),
                    Photos = d.Photos
                        .Select(p => new PhotoClothes { 
                            Color = colors[p.Color], 
                            PhotoURL = p.Url, 
                            IsMain = p.IsMain 
                        })
                        .ToList(),
                    Stocks = stocks
                };
            }).ToList();

            foreach (KeyValuePair<string, int> valuePair in manClothePopularity)
            {
                ClotheItem? clotheItem = items.FirstOrDefault(p => p.Name == valuePair.Key);
                if (clotheItem is null) continue;

                clotheItem.ClothePopularities = new List<ClothePopularity>
                {
                    new ClothePopularity
                    {
                        Id = Guid.NewGuid(),
                        ClotheId = clotheItem.Id,
                        SoldCount = valuePair.Value
                    }
                };
            }

            await context.ClotheItems.AddRangeAsync(items);
            await context.SaveChangesAsync();
        }
    }
}