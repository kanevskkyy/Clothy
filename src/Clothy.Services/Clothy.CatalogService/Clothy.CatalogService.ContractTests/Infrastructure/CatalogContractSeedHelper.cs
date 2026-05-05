using Clothy.CatalogService.DAL.DB;
using Clothy.CatalogService.Domain.Entities.Catalog;
using Clothy.CatalogService.Domain.Entities.Clothe;
using Clothy.CatalogService.Domain.Entities.Stock;
using Microsoft.Extensions.DependencyInjection;

namespace Clothy.CatalogService.ContractTests.Infrastructure;

public static class CatalogContractSeedHelper
{
    public static async Task<SeedData> SeedClotheAsync(IServiceProvider services)
    {
        using IServiceScope scope = services.CreateScope();
        ClothyCatalogDbContext db = scope.ServiceProvider.GetRequiredService<ClothyCatalogDbContext>();

        Brand brand = new Brand
        {
            Id = Guid.NewGuid(),
            Name = "Contract Brand",
            Slug = "contract-brand"
        };
        ClothingType type = new ClothingType
        {
            Id = Guid.NewGuid(),
            Name = "Jacket",
            Slug = "jacket"
        };
        Collection collection = new Collection
        {
            Id = Guid.NewGuid(), 
            Name = "Spring 2026",
            Slug = "spring-2026",
            CreatedAt = DateTime.UtcNow
        };
        Color color = new Color
        {
            Id = Guid.NewGuid(),
            Name = "Black",
            Slug = "black",
            HexCode = "#000000"
        };
        Size size = new Size
        {
            Id = Guid.NewGuid(),
            Name = "M"
        };
        Material material = new Material
        {
            Id = Guid.NewGuid(),
            Name = "Cotton",
            Slug = "cotton"
        };
        Tag tag = new Tag
        {
            Id = Guid.NewGuid(),
            Name = "Casual", 
            Slug = "casual"
        };

        db.Brands.Add(brand);
        db.ClothingTypes.Add(type);
        db.Collections.Add(collection);
        db.Colors.Add(color);
        db.Sizes.Add(size);
        db.Materials.Add(material);
        db.Tags.Add(tag);

        ClotheItem clothe = new ClotheItem
        {
            Id = Guid.NewGuid(),
            Name = "Contract Test Jacket",
            Slug = "contract-test-jacket",
            Description = "Contract test description",
            Price = 150m,
            Gender = Gender.Male,
            BrandId = brand.Id,
            ClothingTypeId = type.Id,
            CollectionId = collection.Id,
            CreatedAt = DateTime.UtcNow,
            Photos = new List<PhotoClothes>
            {
                new PhotoClothes
                {
                    Id = Guid.NewGuid(),
                    PhotoURL = "https://test.com/photo.jpg",
                    ColorId = color.Id,
                    IsMain = true
                }
            },
            ClotheMaterials = new List<ClotheMaterial>
            {
                new ClotheMaterial
                {
                    MaterialId = material.Id,
                    Percentage = 100
                }
            },
            ClotheTags = new List<ClotheTag>
            {
                new ClotheTag
                {
                    TagId = tag.Id
                }
            },
            Stocks = new List<ClothesStock>
            {
                new ClothesStock
                {
                    Id = Guid.NewGuid(),
                    SizeId = size.Id,
                    ColorId = color.Id,
                    Quantity = 10
                }
            }
        };

        db.ClotheItems.Add(clothe);
        await db.SaveChangesAsync();

        return new SeedData(clothe.Id, clothe.Slug, color.Id, size.Id, color.HexCode);
    }

    public static async Task CleanAsync(IServiceProvider services)
    {
        using IServiceScope scope = services.CreateScope();
        ClothyCatalogDbContext db = scope.ServiceProvider.GetRequiredService<ClothyCatalogDbContext>();

        db.ClotheTags.RemoveRange(db.ClotheTags);
        db.ClotheMaterials.RemoveRange(db.ClotheMaterials);
        db.PhotoClothes.RemoveRange(db.PhotoClothes);
        db.ClothesStocks.RemoveRange(db.ClothesStocks);
        db.ClotheItems.RemoveRange(db.ClotheItems);
        db.Brands.RemoveRange(db.Brands);
        db.ClothingTypes.RemoveRange(db.ClothingTypes);
        db.Collections.RemoveRange(db.Collections);
        db.Colors.RemoveRange(db.Colors);
        db.Sizes.RemoveRange(db.Sizes);
        db.Materials.RemoveRange(db.Materials);
        db.Tags.RemoveRange(db.Tags);
        await db.SaveChangesAsync();
    }

    public record SeedData(Guid ClotheId, string Slug, Guid ColorId, Guid SizeId, string HexCode);
}