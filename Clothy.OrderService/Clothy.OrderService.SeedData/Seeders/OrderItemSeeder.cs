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
    public class OrderItemSeeder : ISeeder
    {
        public async Task SeedAsync(IUnitOfWork uow)
        {
            IEnumerable<OrderItem> existingItems = await uow.OrderItems.GetAllAsync();
            if (existingItems.Any()) return;

            IEnumerable<Order> orders = await uow.Orders.GetAllAsync();
            if (!orders.Any()) throw new SeederDependencyException("Orders table must be seeded before seeding OrderItems.");
            
            Faker faker = new Faker();

            foreach (Order order in orders)
            {
                int itemsCount = faker.Random.Int(1, 5);
                for (int i = 0; i < itemsCount; i++)
                {
                    OrderItem item = new OrderItem
                    {
                        OrderId = order.Id,
                        ClotheId = Guid.NewGuid(),
                        ClotheName = faker.Commerce.ProductName(),
                        Price = faker.Random.Double(10, 200),
                        Quantity = faker.Random.Int(1, 3),
                        MainPhoto = faker.Image.PicsumUrl(),
                        ColorId = Guid.NewGuid(),
                        HexCode = $"#{faker.Random.Hexadecimal(6).Substring(2).ToUpper()}",
                        SizeId = Guid.NewGuid(),
                        SizeName = faker.Random.Word(),
                        CreatedAt = faker.Date.Past(2).ToUniversalTime(),
                        UpdatedAt = faker.Date.Recent(30).ToUniversalTime()
                    };

                    await uow.OrderItems.AddWithoutReturningAsync(item);
                }
            }
            await uow.CommitAsync();
        }
    }
}
