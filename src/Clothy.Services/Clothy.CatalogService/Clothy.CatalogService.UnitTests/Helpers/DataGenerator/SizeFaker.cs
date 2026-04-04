using Bogus;
using Clothy.CatalogService.BLL.DTOs.SizeDTOs;
using Clothy.CatalogService.Domain.Entities.Catalog;

namespace Clothy.CatalogService.UnitTests.Helpers;

public static class SizeFaker
{
    public static Size GenerateSize(Guid? id = null)
    {
        return new Faker<Size>()
            .RuleFor(s => s.Id, _ => id ?? Guid.NewGuid())
            .RuleFor(s => s.Name, f => f.PickRandom("XS", "S", "M", "L", "XL", "XXL"))
            .Generate();
    }

    public static List<Size> GenerateSizes(int count = 5)
    {
        return Enumerable.Range(0, count).Select(_ => GenerateSize()).ToList();
    }

    public static SizeReadDTO GenerateReadDTOFromEntity(Size size)
    {
        return new SizeReadDTO
        {
            Id = size.Id,
            Name = size.Name
        };
    }

    public static SizeReadDTO GenerateReadDTO(Guid? id = null)
    {
        return new Faker<SizeReadDTO>()
            .RuleFor(d => d.Id, _ => id ?? Guid.NewGuid())
            .RuleFor(d => d.Name, f => f.PickRandom("XS", "S", "M", "L", "XL", "XXL"))
            .Generate();
    }

    public static SizeCreateDTO GenerateCreateDTO()
    {
        return new Faker<SizeCreateDTO>()
            .RuleFor(d => d.Name, f => f.PickRandom("XS", "S", "M", "L", "XL", "XXL"))
            .Generate();
    }

    public static SizeUpdateDTO GenerateUpdateDTO()
    {
        return new Faker<SizeUpdateDTO>()
            .RuleFor(d => d.Name, f => f.PickRandom("XS", "S", "M", "L", "XL", "XXL"))
            .Generate();
    }
}