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
    public class BrandSeeder : ISeeder
    {
        public async Task SeedAsync(ClothyCatalogDbContext context)
        {
            if (await context.Brands.AnyAsync()) return;

            List<string> brandNames = new List<string>()
            {
                "Nike",
                "Adidas",
                "Puma",
                "Reebok",
                "New Balance",
                "Under Armour",
                "Off-White",
                "Gucci",
                "Louis Vuitton",
                "Balenciaga"
            };

            List<Brand> brands = new List<Brand>();
            Faker faker = new Faker();

            foreach(string brandName in brandNames)
            {
                Brand brand = new Brand
                {
                    Id = Guid.NewGuid(),
                    CreatedAt = faker.Date.Past(2).ToUniversalTime(),
                    Name = brandName,
                    PhotoURL = faker.Image.PicsumUrl()
                };

                brands.Add(brand);
            }

            await context.Brands.AddRangeAsync(brands);
            await context.SaveChangesAsync();
        }
    }
}
