using Bogus;
using Clothy.CatalogService.BLL.DTOs.BrandDTOs;
using Clothy.CatalogService.Domain.Entities.Catalog;

namespace Clothy.CatalogService.UnitTests.Helpers;

public static class BrandFaker
{
    public static Brand GenerateBrand(Guid? id = null)
    {
        return new Faker<Brand>()
            .RuleFor(b => b.Id, _ => id ?? Guid.NewGuid())
            .RuleFor(b => b.Name, f => f.Company.CompanyName())
            .RuleFor(b => b.Slug, (f, b) => b.Name!.ToLower().Replace(" ", "-"))
            .Generate();
    }

    public static List<Brand> GenerateBrands(int count = 5)
    {
        return new Faker<Brand>()
            .RuleFor(b => b.Id, _ => Guid.NewGuid())
            .RuleFor(b => b.Name, f => f.Company.CompanyName())
            .RuleFor(b => b.Slug, (f, b) => b.Name!.ToLower().Replace(" ", "-"))
            .Generate(count);
    }


    public static BrandCreateDTO GenerateCreateDTO()
    {
        return new Faker<BrandCreateDTO>()
            .RuleFor(d => d.Name, f => f.Company.CompanyName())
            .RuleFor(d => d.Slug, (f, d) => d.Name!.ToLower().Replace(" ", "-"))
            .Generate();
    }

    public static BrandUpdateDTO GenerateUpdateDTO()
    {
        return new Faker<BrandUpdateDTO>()
            .RuleFor(d => d.Name, f => f.Company.CompanyName())
            .RuleFor(d => d.Slug, (f, d) => d.Name!.ToLower().Replace(" ", "-"))
            .Generate();
    }

    public static BrandReadDTO GenerateReadDTO(Guid? id = null)
    {
        return new Faker<BrandReadDTO>()
            .RuleFor(d => d.Id, _ => id ?? Guid.NewGuid())
            .RuleFor(d => d.Name, f => f.Company.CompanyName())
            .RuleFor(d => d.Slug, (f, d) => d.Name!.ToLower().Replace(" ", "-"))
            .Generate();
    }

    public static BrandReadDTO GenerateReadDTOFromBrand(Brand brand)
    {
        return new BrandReadDTO
        {
            Id = brand.Id,
            Name = brand.Name,
            Slug = brand.Slug
        };
    }

    public static List<BrandReadDTO> GenerateReadDTOs(int count = 5)
    {
        return Enumerable.Range(0, count)
            .Select(_ => GenerateReadDTO())
            .ToList();
    }
}