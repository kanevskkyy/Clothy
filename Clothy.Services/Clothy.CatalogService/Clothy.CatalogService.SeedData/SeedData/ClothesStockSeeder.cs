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
    public class ClothesStockSeeder : ISeeder
    {
        public async Task SeedAsync(ClothyCatalogDbContext context)
        {
            if (await context.ClothesStocks.AnyAsync()) return;

            List<ClotheItem> clotheItems = await context.ClotheItems.ToListAsync();
            List<Size> sizes = await context.Sizes.ToListAsync();
            List<Color> colors = await context.Colors.ToListAsync();
            Faker faker = new Faker();

            List<ClothesStock> stocks = new List<ClothesStock>();
            HashSet<string> existingCombinations = new HashSet<string>();

            const int ROWS_COUNT = 50;
            while (stocks.Count < ROWS_COUNT)
            {
                ClotheItem clothe = faker.PickRandom(clotheItems);
                Size size = faker.PickRandom(sizes);
                Color color = faker.PickRandom(colors);

                string key = $"{clothe.Id}-{size.Id}-{color.Id}";
                if (!existingCombinations.Contains(key))
                {
                    existingCombinations.Add(key);

                    ClothesStock stock = new ClothesStock
                    {
                        Id = Guid.NewGuid(),
                        ClotheId = clothe.Id,
                        SizeId = size.Id,
                        ColorId = color.Id,
                        Quantity = faker.Random.Int(0, 100),
                        CreatedAt = faker.Date.Past(2).ToUniversalTime()
                    };

                    stocks.Add(stock);
                }
            }

            await context.ClothesStocks.AddRangeAsync(stocks);
            await context.SaveChangesAsync();
        }
    }
}
