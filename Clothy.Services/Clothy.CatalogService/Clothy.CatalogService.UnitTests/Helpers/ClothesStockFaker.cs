using Bogus;
using Clothy.CatalogService.BLL.DTOs.ClotheStocksDTOs;
using Clothy.CatalogService.Domain.Entities.Clothe;
using Clothy.CatalogService.Domain.Entities.Stock;

namespace Clothy.CatalogService.UnitTests.Helpers;

public static class ClothesStockFaker
{
    public static ClothesStock GenerateStock(Guid? id = null, int quantity = 10)
    {
        Faker faker = new Faker();
        string clotheName = faker.Commerce.ProductName();
 
        return new ClothesStock
        {
            Id = id ?? Guid.NewGuid(),
            ClotheId = Guid.NewGuid(),
            SizeId = Guid.NewGuid(),
            ColorId = Guid.NewGuid(),
            Quantity = quantity,
            Clothe = new ClotheItem
            {
                Id = Guid.NewGuid(),
                Name = clotheName,
                Slug = clotheName.ToLower().Replace(" ", "-")
            }
        };
    }
 
    public static ClothesStockReadDTO GenerateReadDTO(Guid? id = null)
    {
        return new Faker<ClothesStockReadDTO>()
            .RuleFor(d => d.Id, _ => id ?? Guid.NewGuid())
            .RuleFor(d => d.Quantity, f => f.Random.Int(0, 100))
            .Generate();
    }
 
    public static ClothesStockUpdateDTO GenerateUpdateDTO(int quantity = 20)
    {
        return new ClothesStockUpdateDTO
        {
            Quantity = quantity
        };
    }
}