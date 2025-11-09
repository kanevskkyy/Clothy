using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bogus;
using Clothy.OrderService.DAL.UOW;
using Clothy.OrderService.Domain.Entities;

namespace Clothy.OrderService.SeedData.Seeders
{
    public class SettlementSeeder : ISeeder
    {
        public async Task SeedAsync(IUnitOfWork uow)
        {
            IEnumerable<Settlement> existingSettlements = await uow.Settlement.GetAllAsync();
            if (existingSettlements.Any()) return;

            IEnumerable<Region> regions = await uow.Region.GetAllAsync();
            if(!regions.Any()) return;

            Faker faker = new Faker();
            List<Settlement> settlements = new List<Settlement>();

            foreach(Region region in regions)
            {
                for(int i = 0; i < 4; i++)
                {
                    settlements.Add(new Settlement
                    {
                        Name = $"{faker.Address.SecondaryAddress()} + {i}",
                        RegionId = region.Id,
                        CreatedAt = faker.Date.Past(5).ToUniversalTime(),
                        UpdatedAt = faker.Date.Recent(30).ToUniversalTime()
                    });
                }
            }

            foreach(Settlement settlement in settlements)
            {
                await uow.Settlement.AddWithoutReturningAsync(settlement);
            }
            await uow.CommitAsync();
        }
    }
}
