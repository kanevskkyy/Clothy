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
    public class DeliveryProviderSeeder : ISeeder
    {
        public async Task SeedAsync(IUnitOfWork uow)
        {
            IEnumerable<DeliveryProvider> existingProviders = await uow.DeliveryProviders.GetAllAsync();
            if (existingProviders.Any()) return;

            List<string> providers = new List<string>
            {
                "Nova Poshta",
                "Ukrposhta",
                "Meest Express",
                "Delivery Service X",
                "Fast Courier"
            };

            Faker faker = new Faker();

            foreach (string providerName in providers)
            {
                DeliveryProvider provider = new DeliveryProvider
                {
                    Name = providerName,
                    IconUrl = faker.Image.PicsumUrl(),
                    CreatedAt = faker.Date.Past(5).ToUniversalTime(),
                    UpdatedAt = faker.Date.Recent(30).ToUniversalTime()
                };

                await uow.DeliveryProviders.AddWithoutReturningAsync(provider);
            }
            await uow.CommitAsync();
        }
    }
}
