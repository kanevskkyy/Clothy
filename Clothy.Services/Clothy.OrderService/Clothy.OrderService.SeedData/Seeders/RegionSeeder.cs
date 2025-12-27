using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bogus;
using Clothy.OrderService.DAL.UOW;
using Clothy.OrderService.Domain.Entities;

namespace Clothy.OrderService.SeedData.Seeders
{
    public class RegionSeeder : ISeeder
    {
        public async Task SeedAsync(IUnitOfWork uow)
        {
            IEnumerable<Region> existingRegions = await uow.Region.GetAllAsync();
            if (existingRegions.Any()) return;

            Faker faker = new Faker();
            List<Region> regions = new List<Region>();

            for (int i = 0; i < 10; i++)
            {
                regions.Add(new Region
                {
                    Name = $"{faker.Address.Country()} Region {i + 1}",
                    Ref = Guid.NewGuid().ToString(), 
                    CreatedAt = faker.Date.Past(5).ToUniversalTime(),
                    UpdatedAt = faker.Date.Recent(30).ToUniversalTime()
                });
            }

            foreach (var region in regions)
            {
                await uow.Region.AddWithoutReturningAsync(region);
            }

            await uow.CommitAsync();
        }
    }
}