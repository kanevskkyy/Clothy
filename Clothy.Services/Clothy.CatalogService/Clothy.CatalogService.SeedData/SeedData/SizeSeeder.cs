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
    public class SizeSeeder : ISeeder
    {
        public async Task SeedAsync(ClothyCatalogDbContext context)
        {
            if (await context.Sizes.AnyAsync()) return;

            List<string> sizeNames = new List<string>()
            {
                "XS",
                "S",
                "M",
                "L",
                "XL",
                "XXL"
            };

            List<Size> sizes = new List<Size>();
            Faker faker = new Faker();

            foreach (string sizeName in sizeNames)
            {
                Size size = new Size
                {
                    Id = Guid.NewGuid(),
                    CreatedAt = faker.Date.Past(2).ToUniversalTime(),
                    Name = sizeName
                };

                sizes.Add(size);
            }

            await context.Sizes.AddRangeAsync(sizes);
            await context.SaveChangesAsync();
        }
    }
}
