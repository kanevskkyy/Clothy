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
    public class PickupPointSeeder : ISeeder
    {
        public async Task SeedAsync(IUnitOfWork uow)
        {
            IEnumerable<PickupPoints> existingPickupPoints = await uow.PickupPoint.GetAllAsync();
            if (existingPickupPoints.Any()) return;

            IEnumerable<DeliveryProvider> providers = await uow.DeliveryProviders.GetAllAsync();
            if (!providers.Any()) return;

            Faker faker = new Faker();

            Faker<PickupPoints> pickupPointFaker = new Faker<PickupPoints>()
                .RuleFor(p => p.Address, f => f.Address.StreetAddress())
                .RuleFor(p => p.DeliveryProviderId, f => f.PickRandom(providers).Id)
                .RuleFor(p => p.CreatedAt, f => f.Date.Past(2).ToUniversalTime())
                .RuleFor(p => p.UpdatedAt, f => f.Date.Recent(10).ToUniversalTime());

            List<PickupPoints> pickupPoints = pickupPointFaker.Generate(30);

            foreach (PickupPoints point in pickupPoints)
            {
                await uow.PickupPoint.AddWithoutReturningAsync(point);
            }

            await uow.CommitAsync();
        }
    }
}
