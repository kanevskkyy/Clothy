using Clothy.CatalogService.DAL.DB;
using Clothy.CatalogService.Domain.Entities.Catalog;
using Clothy.CatalogService.Domain.Entities.Clothe;
using Clothy.CatalogService.Domain.Entities.Stock;
using Clothy.CatalogService.SeedData.SeedData.Production.Helpers;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Clothy.CatalogService.SeedData.SeedData.Production
{
    public class WomanClotheSeeder : ISeeder
    {
        private static Random random = new Random();

        private static Dictionary<string, int> womanClothePopularity = new Dictionary<string, int>()
        {
            {
                "Palm Angels Classic Script Logo Fitted Tee", 996   
            },
            {
                "Gucci GG Logo Intarsia Wool Cardigan", 995
            },
            {
                "Adidas Originals Trefoil Cropped Hoodie", 994
            },
            {
                "BAPE ABC Camo Cropped Zip Hoodie", 993
            }
        };
        
        public async Task SeedAsync(ClothyCatalogDbContext context)
        {
            if (await context.ClotheItems.AnyAsync(p => p.Gender == Gender.Female)) return;

            Dictionary<string, Tag> tags = await context.Tags.ToDictionaryAsync(t => t.Name);
            Dictionary<string, Material> materials = await context.Materials.ToDictionaryAsync(m => m.Name);
            Dictionary<string, Brand> brands = await context.Brands.ToDictionaryAsync(b => b.Name);
            Dictionary<string, ClothingType> types = await context.ClothingTypes.ToDictionaryAsync(t => t.Name);
            Dictionary<string, Collection> collections = await context.Collections.ToDictionaryAsync(c => c.Name);
            Dictionary<string, Color> colors = await context.Colors.ToDictionaryAsync(c => c.Name);
            List<Size> sizes = await context.Sizes.ToListAsync();

            string json = await File.ReadAllTextAsync("SeedData/Production/Data/woman_clothes.json");
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
                    Gender = Gender.Female,
                    Brand = brands[d.Brand],
                    ClothyType = types[d.Type],
                    Collection = collections[d.Collection],
                    ClotheTags = d.Tags
                        .Select(t => new ClotheTag
                        {
                            Tag = tags[t]
                        }).ToList(),
                    ClotheMaterials = d.Materials
                        .Select(m => new ClotheMaterial
                        {
                            Material = materials[m.Name],
                            Percentage = m.Percentage
                        }
                        ).ToList(),
                    Photos = d.Photos
                        .Select(p => new PhotoClothes
                        {
                            Color = colors[p.Color],
                            PhotoURL = p.Url,
                            IsMain = p.IsMain
                        })
                        .ToList(),
                    Stocks = stocks
                };
            }).ToList();
            
            foreach (KeyValuePair<string, int> valuePair in womanClothePopularity)
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
