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
    public class OrderStatusSeeder : ISeeder
    {
        public async Task SeedAsync(IUnitOfWork uow)
        {
            IEnumerable<OrderStatus> existingStatuses = await uow.OrderStatuses.GetAllAsync();
            if (existingStatuses.Any()) return;

            List<string> statuses = new List<string>
            {
                "Pending",
                "Processing",
                "Shipped",
                "Delivered",
                "Cancelled"
            };

            Faker faker = new Faker();

            foreach (string statusName in statuses)
            {
                OrderStatus status = new OrderStatus
                {
                    Name = statusName,
                    IconUrl = faker.Image.PicsumUrl(),
                    CreatedAt = faker.Date.Past(5).ToUniversalTime(),
                    UpdatedAt = faker.Date.Recent(30).ToUniversalTime()
                };

                await uow.OrderStatuses.AddWithoutReturningAsync(status);
            }
            await uow.CommitAsync();
        }
    }
}
