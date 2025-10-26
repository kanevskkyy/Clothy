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
    public class CollectionSeeder : ISeeder
    {
        public async Task SeedAsync(ClothyCatalogDbContext context)
        {
            if (await context.Collections.AnyAsync()) return;

            List<string> collectionNames = new List<string>()
            {
                "Nike x Off-White",
                "Adidas x Supreme",
                "Puma x Balenciaga",
                "Reebok x Vetements",
                "New Balance x Kith",
                "Under Armour x BAPE",
                "Gucci x The North Face",
                "Louis Vuitton x Fragment",
                "Jordan x Dior",
                "Converse x Comme des Garçons"
            };

            List<Collection> collections = new List<Collection>();
            Faker faker = new Faker();

            foreach (string collectionName in collectionNames)
            {
                Collection collection = new Collection
                {
                    Id = Guid.NewGuid(),
                    CreatedAt = faker.Date.Past(2).ToUniversalTime(),
                    Name = collectionName,
                    Slug = collectionName.ToLower().Replace(" ", "-").Replace("x", "x")
                };

                collections.Add(collection);
            }

            await context.Collections.AddRangeAsync(collections);
            await context.SaveChangesAsync();
        }
    }
}
