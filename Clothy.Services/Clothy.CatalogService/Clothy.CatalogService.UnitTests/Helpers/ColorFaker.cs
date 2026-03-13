using Bogus;
using Clothy.CatalogService.BLL.DTOs.ColorDTOs;
using Clothy.CatalogService.Domain.Entities.Catalog;

namespace Clothy.CatalogService.UnitTests.Helpers;

public static class ColorFaker
{
    public static Color GenerateColor(Guid? id = null)
    {
        return new Faker<Color>()
            .RuleFor(c => c.Id, _ => id ?? Guid.NewGuid())
            .RuleFor(c => c.Name, f => f.Commerce.Color())
            .RuleFor(c => c.HexCode, f => f.Internet.Color())
            .RuleFor(c => c.Slug, (f, c) => c.Name!.ToLower().Replace(" ", "-"))
            .Generate();
    }

    public static List<Color> GenerateColors(int count = 5)
    {
        return Enumerable.Range(0, count).Select(_ => GenerateColor()).ToList();
    }

    public static ColorReadDTO GenerateReadDTOFromEntity(Color color)
    {
        return new ColorReadDTO
        {
            Id = color.Id,
            Name = color.Name,
            HexCode = color.HexCode,
            Slug = color.Slug
        };
    }

    public static ColorReadDTO GenerateReadDTO(Guid? id = null)
    {
        return new Faker<ColorReadDTO>()
            .RuleFor(d => d.Id, _ => id ?? Guid.NewGuid())
            .RuleFor(d => d.Name, f => f.Commerce.Color())
            .RuleFor(d => d.HexCode, f => f.Internet.Color())
            .RuleFor(d => d.Slug, (f, d) => d.Name!.ToLower().Replace(" ", "-"))
            .Generate();
    }

    public static ColorCreateDTO GenerateCreateDTO()
    {
        return new Faker<ColorCreateDTO>()
            .RuleFor(d => d.Name, f => f.Commerce.Color())
            .RuleFor(d => d.HexCode, f => f.Internet.Color())
            .RuleFor(d => d.Slug, (f, d) => d.Name!.ToLower().Replace(" ", "-"))
            .Generate();
    }

    public static ColorUpdateDTO GenerateUpdateDTO()
    {
        return new Faker<ColorUpdateDTO>()
            .RuleFor(d => d.Name, f => f.Commerce.Color())
            .RuleFor(d => d.HexCode, f => f.Internet.Color())
            .RuleFor(d => d.Slug, (f, d) => d.Name!.ToLower().Replace(" ", "-"))
            .Generate();
    }
}