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
        public async Task SeedAsync(IUnitOfWork unitOfWork)
        {
            IEnumerable<OrderItem> existingItems = await unitOfWork.OrderItems.GetAllAsync();
            if (existingItems.Any()) return;

            IEnumerable<Order> orders = await unitOfWork.Orders.GetAllAsync();
            if (!orders.Any()) throw new SeederDependencyException("Orders table must be seeded before seeding OrderItems.");
            
            Faker faker = new Faker();
            
            List<string> sizes = new List<string>
            { 
                "XS", 
                "S", 
                "M", 
                "L", 
                "XL", 
                "XXL" 
            };

            List<string> photosUrl = new List<string>
            {
                "https://res.cloudinary.com/dkdljnfja/image/upload/v1772191110/1_rxt3m6.webp",
                "https://res.cloudinary.com/dkdljnfja/image/upload/v1772190844/palm_angels_white_3_emwx5y.jpg",
                "https://res.cloudinary.com/dkdljnfja/image/upload/v1772190533/balenciaga_1_fqwer0.jpg",
                "https://res.cloudinary.com/dkdljnfja/image/upload/v1772190319/nike_1_ncatfi.webp",
                "https://res.cloudinary.com/dkdljnfja/image/upload/v1772189947/stone_island_1_wqz5cv.avif",
                "https://res.cloudinary.com/dkdljnfja/image/upload/v1772186696/nike_blue_1_jdkop6.jpg",
                "https://res.cloudinary.com/dkdljnfja/image/upload/v1772186295/nike_black_1_s2kcmg.webp",
                "https://res.cloudinary.com/dkdljnfja/image/upload/v1772185900/off_white_blue_1_sgd5gt.webp",
                "https://res.cloudinary.com/dkdljnfja/image/upload/v1772198246/1_tnd0cf.webp",
                "https://res.cloudinary.com/dkdljnfja/image/upload/v1772198052/1_wg6oi0.webp",
                "https://res.cloudinary.com/dkdljnfja/image/upload/v1772197872/1_yhoqrk.jpg",
                "https://res.cloudinary.com/dkdljnfja/image/upload/v1772197583/2_fddzvb.avif",
                "https://res.cloudinary.com/dkdljnfja/image/upload/v1772196704/1_rkpsoe.avif",
                "https://res.cloudinary.com/dkdljnfja/image/upload/v1772196327/1_ehrfjn.avif",
                "https://res.cloudinary.com/dkdljnfja/image/upload/v1772195936/1_f9o9yz.webp",
                "https://res.cloudinary.com/dkdljnfja/image/upload/v1772195558/1_n9bwkh.webp",
                "https://res.cloudinary.com/dkdljnfja/image/upload/v1772195441/1_or3pht.jpg"
            };

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
                        Price = faker.Random.Decimal(10, 200),
                        Quantity = faker.Random.Int(1, 3),
                        MainPhoto = faker.PickRandom(photosUrl),
                        ColorId = Guid.NewGuid(),
                        HexCode = $"#{faker.Random.Hexadecimal(6).Substring(2).ToUpper()}",
                        SizeId = Guid.NewGuid(),
                        SizeName = faker.PickRandom(sizes),
                        CreatedAt = faker.Date.Past(2).ToUniversalTime(),
                        UpdatedAt = faker.Date.Recent(30).ToUniversalTime()
                    };

                    await unitOfWork.OrderItems.AddWithoutReturningAsync(item);
                }
            }
            await unitOfWork.CommitAsync();
        }
    }
}
