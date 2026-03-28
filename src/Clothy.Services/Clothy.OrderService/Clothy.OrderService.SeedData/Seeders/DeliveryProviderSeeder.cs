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
            if (existingProviders.Count() > 1) return;

            Dictionary<string, string> providers = new Dictionary<string, string>
            {
                {
                    "Ukrposhta", "https://res.cloudinary.com/dkdljnfja/image/upload/v1772204480/pin_2_lbzhuo.png"
                },
                {
                    "Meest Express", "https://res.cloudinary.com/dkdljnfja/image/upload/v1772204523/share_yizm42.png"
                },
                {
                    "Fast Delivery", "https://res.cloudinary.com/dkdljnfja/image/upload/v1772204607/pngtree-fast-delivery-label-design-vector-png-image_7087605_xqmdbg.png"
                },
            };

            Faker faker = new Faker();

            foreach (KeyValuePair<string, string> provider in providers)
            {
                DeliveryProvider deliveryProvider = new DeliveryProvider
                {
                    Name = provider.Key,
                    IconUrl = provider.Value,
                    CreatedAt = faker.Date.Past(5).ToUniversalTime(),
                    UpdatedAt = faker.Date.Recent(30).ToUniversalTime()
                };

                await uow.DeliveryProviders.AddWithoutReturningAsync(deliveryProvider);
            }
            await uow.CommitAsync();
        }
    }
}