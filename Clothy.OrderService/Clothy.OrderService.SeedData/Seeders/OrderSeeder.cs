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
    public class OrderSeeder : ISeeder
    {
        public async Task SeedAsync(IUnitOfWork uow)
        {
            IEnumerable<Order> existingOrders = await uow.Orders.GetAllAsync();
            if (existingOrders.Any()) return;
            
            IEnumerable<OrderStatus> statuses = await uow.OrderStatuses.GetAllAsync();
            if (!statuses.Any()) throw new SeederDependencyException("OrderStatus table must be seeded before seeding Orders.");
            
            Faker faker = new Faker();

            for (int i = 0; i < 10; i++)
            {
                Order order = new Order
                {
                    StatusId = faker.PickRandom(statuses).Id,
                    UserId = Guid.NewGuid(),
                    UserFirstName = faker.Name.FirstName(),
                    UserLastName = faker.Name.LastName(),
                    CreatedAt = faker.Date.Past(2).ToUniversalTime(),
                    UpdatedAt = faker.Date.Recent(30).ToUniversalTime()
                };

                await uow.Orders.AddWithoutReturningAsync(order);
            }
            await uow.CommitAsync();
        }
    }
}
