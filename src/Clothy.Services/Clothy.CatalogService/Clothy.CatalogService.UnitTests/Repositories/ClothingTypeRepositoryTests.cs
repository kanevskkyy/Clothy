using Clothy.CatalogService.DAL.DB;
using Clothy.CatalogService.DAL.Repositories;
using Clothy.CatalogService.Domain.Entities.Catalog;
using Clothy.CatalogService.Domain.Entities.Clothe;
using Clothy.CatalogService.UnitTests.Helpers;
using Clothy.CatalogService.UnitTests.Helpers.Persistance;
using FluentAssertions;
using Xunit;

namespace Clothy.CatalogService.UnitTests.Repositories;

public class ClothingTypeRepositoryTests : IDisposable
{
    private ClothyCatalogDbContext context;
    private ClothingTypeRepository repository;
 
    public ClothingTypeRepositoryTests()
    {
        context = DbContextFactory.CreateInMemoryContext();
        repository = new ClothingTypeRepository(context);
    }
 
    public void Dispose() => context.Dispose();
 
    [Fact]
    public async Task ExistsAsync_WhenExists_ReturnsTrue()
    {
        ClothingType type = ClothingTypeFaker.GenerateClothingType();
        await context.ClothingTypes.AddAsync(type);
        await context.SaveChangesAsync();
 
        bool result = await repository.ExistsAsync(type.Id);
 
        result.Should().BeTrue();
    }
 
    [Fact]
    public async Task ExistsAsync_WhenDoesNotExist_ReturnsFalse()
    {
        bool result = await repository.ExistsAsync(Guid.NewGuid());
        result.Should().BeFalse();
    }
    
    [Fact]
    public async Task IsNameAlreadyExistsAsync_WhenNameExists_AndNoId_ReturnsTrue()
    {
        ClothingType type = ClothingTypeFaker.GenerateClothingType();
        type.Name = "Jackets";
        await context.ClothingTypes.AddAsync(type);
        await context.SaveChangesAsync();
 
        bool result = await repository.IsNameAlreadyExistsAsync("Jackets");
        result.Should().BeTrue();
    }
 
    [Fact]
    public async Task IsNameAlreadyExistsAsync_WhenNameBelongsToSameEntity_ReturnsFalse()
    {
        ClothingType type = ClothingTypeFaker.GenerateClothingType();
        type.Name = "Jackets";
        await context.ClothingTypes.AddAsync(type);
        await context.SaveChangesAsync();
 
        bool result = await repository.IsNameAlreadyExistsAsync("Jackets", type.Id);
        result.Should().BeFalse();
    }
 
    [Fact]
    public async Task IsNameAlreadyExistsAsync_IsCaseInsensitive()
    {
        ClothingType type = ClothingTypeFaker.GenerateClothingType();
        type.Name = "jackets";
        await context.ClothingTypes.AddAsync(type);
        await context.SaveChangesAsync();
 
        bool result = await repository.IsNameAlreadyExistsAsync("JACKETS");
        result.Should().BeTrue();
    }
    
    [Fact]
    public async Task IsSlugAlreadyExistsAsync_WhenSlugExists_ReturnsTrue()
    {
        ClothingType type = ClothingTypeFaker.GenerateClothingType();
        type.Slug = "my-slug";
        await context.ClothingTypes.AddAsync(type);
        await context.SaveChangesAsync();
 
        bool result = await repository.IsSlugAlreadyExistsAsync("my-slug");
        result.Should().BeTrue();
    }
 
    [Fact]
    public async Task IsSlugAlreadyExistsAsync_WhenSlugBelongsToSameEntity_ReturnsFalse()
    {
        ClothingType type = ClothingTypeFaker.GenerateClothingType();
        type.Slug = "my-slug";
        await context.ClothingTypes.AddAsync(type);
        await context.SaveChangesAsync();
 
        bool result = await repository.IsSlugAlreadyExistsAsync("my-slug", type.Id);
        result.Should().BeFalse();
    }
}