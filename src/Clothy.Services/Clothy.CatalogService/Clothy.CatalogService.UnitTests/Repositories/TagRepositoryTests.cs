using Clothy.CatalogService.DAL.DB;
using Clothy.CatalogService.DAL.Repositories;
using Clothy.CatalogService.Domain.Entities.Catalog;
using Clothy.CatalogService.Domain.Entities.Clothe;
using Clothy.CatalogService.UnitTests.Helpers;
using Clothy.CatalogService.UnitTests.Helpers.Persistance;
using FluentAssertions;
using Xunit;

namespace Clothy.CatalogService.UnitTests.Repositories;

public class TagRepositoryTests : IDisposable
{
    private ClothyCatalogDbContext
        context;

    private TagRepository repository;

    public TagRepositoryTests()
    {
        context = DbContextFactory.CreateInMemoryContext();
        repository = new TagRepository(context);
    }

    public void Dispose() => context.Dispose();

    [Fact]
    public async Task IsNameAlreadyExistsAsync_WhenNameExists_ReturnsTrue()
    {
        Tag tag = TagFaker.GenerateTag();
        tag.Name = "Sale";
        await context.Tags.AddAsync(tag);
        await context.SaveChangesAsync();

        bool result = await repository.IsNameAlreadyExistsAsync("Sale");
        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsNameAlreadyExistsAsync_WhenNameBelongsToSameEntity_ReturnsFalse()
    {
        Tag tag = TagFaker.GenerateTag();
        tag.Name = "Sale";
        await context.Tags.AddAsync(tag);
        await context.SaveChangesAsync();

        bool result = await repository.IsNameAlreadyExistsAsync("Sale", tag.Id);
        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsSlugAlreadyExistsAsync_WhenSlugExists_ReturnsTrue()
    {
        Tag tag = TagFaker.GenerateTag();
        tag.Slug = "sale";
        await context.Tags.AddAsync(tag);
        await context.SaveChangesAsync();

        bool result = await repository.IsSlugAlreadyExistsAsync("sale");
        result.Should().BeTrue();
    }

    [Fact]
    public async Task AreAllExistAsync_WhenAllIdsExist_ReturnsTrue()
    {
        List<Tag> tags = TagFaker.GenerateTags(3);
        await context.Tags.AddRangeAsync(tags);
        await context.SaveChangesAsync();

        bool result = await repository.AreAllExistAsync(tags.Select(t => t.Id));
        result.Should().BeTrue();
    }

    [Fact]
    public async Task AreAllExistAsync_WhenSomeIdsDoNotExist_ReturnsFalse()
    {
        List<Tag> tags = TagFaker.GenerateTags(2);
        await context.Tags.AddRangeAsync(tags);
        await context.SaveChangesAsync();

        List<Guid> ids = tags.Select(t => t.Id).Append(Guid.NewGuid()).ToList();

        bool result = await repository.AreAllExistAsync(ids);
        result.Should().BeFalse();
    }

    [Fact]
    public async Task AreAllExistAsync_WhenEmptyCollection_ReturnsTrue()
    {
        bool result = await repository.AreAllExistAsync(Enumerable.Empty<Guid>());
        result.Should().BeTrue();
    }
}