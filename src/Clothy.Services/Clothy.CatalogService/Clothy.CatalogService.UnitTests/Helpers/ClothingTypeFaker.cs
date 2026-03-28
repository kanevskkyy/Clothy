using Bogus;
using Clothy.CatalogService.BLL.DTOs.ClothingTypeDTOs;
using Clothy.CatalogService.Domain.Entities.Catalog;

namespace Clothy.CatalogService.UnitTests.Helpers;

public static class ClothingTypeFaker
{
    public static ClothingType GenerateClothingType(Guid? id = null)
    {
        return new Faker<ClothingType>()
            .RuleFor(c => c.Id, _ => id ?? Guid.NewGuid())
            .RuleFor(c => c.Name, f => f.Commerce.Categories(1)[0])
            .RuleFor(c => c.Slug, (f, c) => c.Name!.ToLower().Replace(" ", "-"))
            .Generate();
    }

    public static List<ClothingType> GenerateClothingTypes(int count = 5)
    {
        return Enumerable.Range(0, count).Select(_ => GenerateClothingType()).ToList();
    }
 
    public static ClothingTypeReadDTO GenerateReadDTO(Guid? id = null)
    {
        return new Faker<ClothingTypeReadDTO>()
            .RuleFor(d => d.Id, _ => id ?? Guid.NewGuid())
            .RuleFor(d => d.Name, f => f.Commerce.Categories(1)[0])
            .RuleFor(d => d.Slug, (f, d) => d.Name!.ToLower().Replace(" ", "-"))
            .Generate();
    }
 
    public static ClothingTypeReadDTO GenerateReadDTOFromEntity(ClothingType clothingType)
    {
        return new ClothingTypeReadDTO
        {
            Id = clothingType.Id,
            Name = clothingType.Name,
            Slug = clothingType.Slug
        };
    }
 
    public static List<ClothingTypeReadDTO> GenerateReadDTOs(int count = 5)
    {
        return Enumerable.Range(0, count)
            .Select(_ => GenerateReadDTO())
            .ToList();
    }
 
    public static ClothingTypeCreateDTO GenerateCreateDTO()
    {
        return new Faker<ClothingTypeCreateDTO>()
            .RuleFor(d => d.Name, f => f.Commerce.Categories(1)[0])
            .RuleFor(d => d.Slug, (f, d) => d.Name!.ToLower().Replace(" ", "-"))
            .Generate();
    }
 
    public static ClothingTypeUpdateDTO GenerateUpdateDTO()
    {
        return new Faker<ClothingTypeUpdateDTO>()
            .RuleFor(d => d.Name, f => f.Commerce.Categories(1)[0])
            .RuleFor(d => d.Slug, (f, d) => d.Name!.ToLower().Replace(" ", "-"))
            .Generate();
    }
}