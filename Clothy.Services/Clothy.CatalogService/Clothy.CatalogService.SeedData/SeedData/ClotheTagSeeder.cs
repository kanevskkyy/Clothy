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
    public class ClotheTagSeeder : ISeeder
    {
        public async Task SeedAsync(ClothyCatalogDbContext context)
        {
            if (await context.ClotheTags.AnyAsync()) return;

            List<ClotheItem> clotheItems = await context.ClotheItems.ToListAsync();
            List<Tag> tags = await context.Tags.ToListAsync();
            Faker faker = new Faker();

            List<ClotheTag> clotheTags = new List<ClotheTag>();
            HashSet<string> existingPairs = new HashSet<string>();

            const int ROWS_COUNT = 50;

            while (clotheTags.Count < ROWS_COUNT)
            {
                ClotheItem clothe = faker.PickRandom(clotheItems);
                Tag tag = faker.PickRandom(tags);

                string key = $"{clothe.Id}-{tag.Id}";
                if (!existingPairs.Contains(key))
                {
                    existingPairs.Add(key);

                    ClotheTag clotheTag = new ClotheTag
                    {
                        ClotheId = clothe.Id,
                        TagId = tag.Id
                    };

                    clotheTags.Add(clotheTag);
                }
            }

            await context.ClotheTags.AddRangeAsync(clotheTags);
            await context.SaveChangesAsync();
        }
    }
}
