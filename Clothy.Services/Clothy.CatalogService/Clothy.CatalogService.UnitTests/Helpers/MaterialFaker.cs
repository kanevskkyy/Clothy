using Bogus;
using Clothy.CatalogService.BLL.DTOs.MaterialDTOs;
using Clothy.CatalogService.Domain.Entities.Catalog;

namespace Clothy.CatalogService.UnitTests.Helpers;

public static class MaterialFaker
{
    public static Material GenerateMaterial(Guid? id = null)
    {
        return new Faker<Material>()
            .RuleFor(m => m.Id, _ => id ?? Guid.NewGuid())
            .RuleFor(m => m.Name, f => f.Commerce.ProductMaterial())
            .RuleFor(m => m.Slug, (f, m) => m.Name!.ToLower().Replace(" ", "-"))
            .Generate();
    }

    public static List<Material> GenerateMaterials(int count = 5)
    {
        return Enumerable.Range(0, count).Select(_ => GenerateMaterial()).ToList();
    }

    public static MaterialReadDTO GenerateReadDTOFromEntity(Material material)
    {
        return new MaterialReadDTO
        {
            Id = material.Id,
            Name = material.Name,
            Slug = material.Slug
        };
    }

    public static MaterialReadDTO GenerateReadDTO(Guid? id = null)
    {
        return new Faker<MaterialReadDTO>()
            .RuleFor(d => d.Id, _ => id ?? Guid.NewGuid())
            .RuleFor(d => d.Name, f => f.Commerce.ProductMaterial())
            .RuleFor(d => d.Slug, (f, d) => d.Name!.ToLower().Replace(" ", "-"))
            .Generate();
    }

    public static MaterialCreateDTO GenerateCreateDTO()
    {
        return new Faker<MaterialCreateDTO>()
            .RuleFor(d => d.Name, f => f.Commerce.ProductMaterial())
            .RuleFor(d => d.Slug, (f, d) => d.Name!.ToLower().Replace(" ", "-"))
            .Generate();
    }

    public static MaterialUpdateDTO GenerateUpdateDTO()
    {
        return new Faker<MaterialUpdateDTO>()
            .RuleFor(d => d.Name, f => f.Commerce.ProductMaterial())
            .RuleFor(d => d.Slug, (f, d) => d.Name!.ToLower().Replace(" ", "-"))
            .Generate();
    }
}