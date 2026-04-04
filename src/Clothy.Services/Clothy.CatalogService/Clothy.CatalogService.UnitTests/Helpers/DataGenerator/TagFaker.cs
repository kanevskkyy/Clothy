using Bogus;
using Clothy.CatalogService.BLL.DTOs.TagDTOs;
using Clothy.CatalogService.Domain.Entities.Catalog;

namespace Clothy.CatalogService.UnitTests.Helpers;

public static class TagFaker
{
    public static Tag GenerateTag(Guid? id = null)
    {
        return new Faker<Tag>()
            .RuleFor(t => t.Id, _ => id ?? Guid.NewGuid())
            .RuleFor(t => t.Name, f => f.Commerce.Categories(1)[0])
            .RuleFor(t => t.Slug, (f, t) => t.Name!.ToLower().Replace(" ", "-"))
            .Generate();
    }

    public static List<Tag> GenerateTags(int count = 5)
    {
        return Enumerable.Range(0, count).Select(_ => GenerateTag()).ToList();
    }

    public static TagReadDTO GenerateReadDTOFromEntity(Tag tag)
    {
        return new TagReadDTO
        {
            Id = tag.Id,
            Name = tag.Name,
            Slug = tag.Slug
        };
    }

    public static TagReadDTO GenerateReadDTO(Guid? id = null)
    {
        return new Faker<TagReadDTO>()
            .RuleFor(d => d.Id, _ => id ?? Guid.NewGuid())
            .RuleFor(d => d.Name, f => f.Commerce.Categories(1)[0])
            .RuleFor(d => d.Slug, (f, d) => d.Name!.ToLower().Replace(" ", "-"))
            .Generate();
    }

    public static TagCreateDTO GenerateCreateDTO()
    {
        return new Faker<TagCreateDTO>()
            .RuleFor(d => d.Name, f => f.Commerce.Categories(1)[0])
            .RuleFor(d => d.Slug, (f, d) => d.Name!.ToLower().Replace(" ", "-"))
            .Generate();
    }

    public static TagUpdateDTO GenerateUpdateDTO()
    {
        return new Faker<TagUpdateDTO>()
            .RuleFor(d => d.Name, f => f.Commerce.Categories(1)[0])
            .RuleFor(d => d.Slug, (f, d) => d.Name!.ToLower().Replace(" ", "-"))
            .Generate();
    }
}