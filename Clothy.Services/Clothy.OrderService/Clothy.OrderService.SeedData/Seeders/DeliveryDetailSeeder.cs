using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bogus;
using Clothy.OrderService.DAL.UOW;
using Clothy.OrderService.Domain.Entities;
using Clothy.OrderService.SeedData.Exceptions;

namespace Clothy.OrderService.SeedData.Seeders
{
    public class DeliveryDetailSeeder : ISeeder
    {
        public async Task SeedAsync(IUnitOfWork uow)
        {
            IEnumerable<DeliveryDetail> existingDeliveryDetails = await uow.DeliveryDetails.GetAllAsync();
            if (existingDeliveryDetails.Any())
            {
                return;
            }

            IEnumerable<Order> orders = await uow.Orders.GetAllAsync();
            IEnumerable<DeliveryProvider> providers = await uow.DeliveryProviders.GetAllAsync();
            IEnumerable<City> cities = await uow.Cities.GetAllAsync();
            IEnumerable<PickupPoints> pickupPoints = await uow.PickupPoint.GetAllAsync();

            if (!orders.Any()) throw new SeederDependencyException("Orders table must be seeded before seeding DeliveryDetails.");
            if (!providers.Any()) throw new SeederDependencyException("DeliveryProvider table must be seeded before seeding DeliveryDetails.");
            if (!cities.Any()) throw new SeederDependencyException("City table must be seeded before seeding DeliveryDetails.");
            if (!pickupPoints.Any()) throw new SeederDependencyException("PickupPoint table must be seeded before seeding DeliveryDetails.");

            Faker faker = new Faker();

            for (int i = 0; i < 20; i++)
            {
                DeliveryDetail detail = new DeliveryDetail
                {
                    OrderId = faker.PickRandom(orders).Id,
                    PhoneNumber = faker.Phone.PhoneNumber("+380#########"),
                    FirstName = faker.Name.FirstName(),
                    LastName = faker.Name.LastName(),
                    PickupPointId = faker.PickRandom(pickupPoints).Id,
                    MiddleName = faker.Name.LastName(),
                    CreatedAt = faker.Date.Past(2).ToUniversalTime(),
                    UpdatedAt = faker.Date.Recent(30).ToUniversalTime()
                };

                await uow.DeliveryDetails.AddWithoutReturningAsync(detail);
            }
            await uow.CommitAsync();
        }
    }
}