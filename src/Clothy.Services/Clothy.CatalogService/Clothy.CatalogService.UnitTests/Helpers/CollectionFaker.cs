using Bogus;
using Clothy.CatalogService.BLL.DTOs.CollectionDTOs;
using Clothy.CatalogService.Domain.Entities.Catalog;

namespace Clothy.CatalogService.UnitTests.Helpers;

public static class CollectionFaker
{
    public static Collection GenerateCollection(Guid? id = null)
    {
        return new Faker<Collection>()
            .RuleFor(c => c.Id, _ => id ?? Guid.NewGuid())
            .RuleFor(c => c.Name, f => f.Commerce.Department())
            .RuleFor(c => c.Slug, (f, c) => c.Name!.ToLower().Replace(" ", "-"))
            .Generate();
    }

    public static List<Collection> GenerateCollections(int count = 5)
    {
        return Enumerable.Range(0, count).Select(_ => GenerateCollection()).ToList();
    }

    public static CollectionReadDTO GenerateReadDTOFromEntity(Collection collection)
    {
        return new CollectionReadDTO
        {
            Id = collection.Id,
            Name = collection.Name,
            Slug = collection.Slug
        };
    }

    public static CollectionReadDTO GenerateReadDTO(Guid? id = null)
    {
        return new Faker<CollectionReadDTO>()
            .RuleFor(d => d.Id, _ => id ?? Guid.NewGuid())
            .RuleFor(d => d.Name, f => f.Commerce.Department())
            .RuleFor(d => d.Slug, (f, d) => d.Name!.ToLower().Replace(" ", "-"))
            .Generate();
    }

    public static CollectionCreateDTO GenerateCreateDTO()
    {
        return new Faker<CollectionCreateDTO>()
            .RuleFor(d => d.Name, f => f.Commerce.Department())
            .RuleFor(d => d.Slug, (f, d) => d.Name!.ToLower().Replace(" ", "-"))
            .Generate();
    }

    public static CollectionUpdateDTO GenerateUpdateDTO()
    {
        return new Faker<CollectionUpdateDTO>()
            .RuleFor(d => d.Name, f => f.Commerce.Department())
            .RuleFor(d => d.Slug, (f, d) => d.Name!.ToLower().Replace(" ", "-"))
            .Generate();
    }
}