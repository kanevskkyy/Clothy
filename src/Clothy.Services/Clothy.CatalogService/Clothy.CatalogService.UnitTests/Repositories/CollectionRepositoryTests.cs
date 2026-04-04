using Clothy.CatalogService.DAL.DB;
using Clothy.CatalogService.DAL.Repositories;
using Clothy.CatalogService.Domain.Entities.Catalog;
using Clothy.CatalogService.Domain.Entities.Clothe;
using Clothy.CatalogService.UnitTests.Helpers;
using Clothy.CatalogService.UnitTests.Helpers.Persistance;
using FluentAssertions;
using Xunit;

namespace Clothy.CatalogService.UnitTests.Repositories;

public class CollectionRepositoryTests : IDisposable
{
    private ClothyCatalogDbContext context;
    private CollectionRepository repository;
 
    public CollectionRepositoryTests()
    {
        context = DbContextFactory.CreateInMemoryContext();
        repository = new CollectionRepository(context);
    }
 
    public void Dispose() => context.Dispose();
 
    [Fact]
    public async Task ExistsAsync_WhenExists_ReturnsTrue()
    {
        Collection collection = CollectionFaker.GenerateCollection();
        await context.Collections.AddAsync(collection);
        await context.SaveChangesAsync();
 
        bool result = await repository.ExistsAsync(collection.Id);
 
        result.Should().BeTrue();
    }
 
    [Fact]
    public async Task ExistsAsync_WhenDoesNotExist_ReturnsFalse()
    {
        bool result = await repository.ExistsAsync(Guid.NewGuid());
        result.Should().BeFalse();
    }
 
    [Fact]
    public async Task IsNameAlreadyExistsAsync_WhenNameExists_ReturnsTrue()
    {
        Collection collection = CollectionFaker.GenerateCollection();
        collection.Name = "Summer 2024";
        await context.Collections.AddAsync(collection);
        await context.SaveChangesAsync();
 
        bool result = await repository.IsNameAlreadyExistsAsync("Summer 2024");
        result.Should().BeTrue();
    }
 
    [Fact]
    public async Task IsNameAlreadyExistsAsync_WhenNameBelongsToSameEntity_ReturnsFalse()
    {
        Collection collection = CollectionFaker.GenerateCollection();
        collection.Name = "Summer 2024";
        await context.Collections.AddAsync(collection);
        await context.SaveChangesAsync();
 
        bool result = await repository.IsNameAlreadyExistsAsync("Summer 2024", collection.Id);
        result.Should().BeFalse();
    }
 
    [Fact]
    public async Task IsSlugAlreadyExistsAsync_WhenSlugExists_ReturnsTrue()
    {
        Collection collection = CollectionFaker.GenerateCollection();
        collection.Slug = "summer-2024";
        await context.Collections.AddAsync(collection);
        await context.SaveChangesAsync();
 
        bool result = await repository.IsSlugAlreadyExistsAsync("summer-2024");
        result.Should().BeTrue();
    }
 
    [Fact]
    public async Task IsSlugAlreadyExistsAsync_WhenSlugBelongsToSameEntity_ReturnsFalse()
    {
        Collection collection = CollectionFaker.GenerateCollection();
        collection.Slug = "summer-2024";
        await context.Collections.AddAsync(collection);
        await context.SaveChangesAsync();
 
        bool result = await repository.IsSlugAlreadyExistsAsync("summer-2024", collection.Id);
        result.Should().BeFalse();
    }
}