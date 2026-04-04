using Clothy.CatalogService.DAL.DB;
using Clothy.CatalogService.DAL.Repositories;
using Clothy.CatalogService.Domain.Entities.Catalog;
using Clothy.CatalogService.UnitTests.Helpers;
using Clothy.CatalogService.UnitTests.Helpers.Persistance;
using FluentAssertions;
using Xunit;

namespace Clothy.CatalogService.UnitTests.Repositories;

public class SizeRepositoryTests : IDisposable
{
    private ClothyCatalogDbContext context;
    private SizeRepository repository;
 
    public SizeRepositoryTests()
    {
        context = DbContextFactory.CreateInMemoryContext();
        repository = new SizeRepository(context);
    }
 
    public void Dispose() => context.Dispose();
 
    [Fact]
    public async Task IsNameAlreadyExistsAsync_WhenNameExists_AndNoId_ReturnsTrue()
    {
        Size size = SizeFaker.GenerateSize();
        size.Name = "XL";
        await context.Sizes.AddAsync(size);
        await context.SaveChangesAsync();
 
        bool result = await repository.IsNameAlreadyExistsAsync("XL");
        result.Should().BeTrue();
    }
 
    [Fact]
    public async Task IsNameAlreadyExistsAsync_WhenNameDoesNotExist_ReturnsFalse()
    {
        bool result = await repository.IsNameAlreadyExistsAsync("XXXL");
        result.Should().BeFalse();
    }
 
    [Fact]
    public async Task IsNameAlreadyExistsAsync_WhenNameBelongsToSameEntity_ReturnsFalse()
    {
        Size size = SizeFaker.GenerateSize();
        size.Name = "XL";
        await context.Sizes.AddAsync(size);
        await context.SaveChangesAsync();
 
        bool result = await repository.IsNameAlreadyExistsAsync("XL", size.Id);
        result.Should().BeFalse();
    }
 
    [Fact]
    public async Task IsNameAlreadyExistsAsync_WhenNameExistsForDifferentEntity_ReturnsTrue()
    {
        Size size1 = SizeFaker.GenerateSize();
        size1.Name = "XL";
        Size size2 = SizeFaker.GenerateSize();
        size2.Name = "L";
        await context.Sizes.AddRangeAsync(size1, size2);
        await context.SaveChangesAsync();
 
        bool result = await repository.IsNameAlreadyExistsAsync("XL", size2.Id);
        result.Should().BeTrue();
    }
 
    [Fact]
    public async Task IsNameAlreadyExistsAsync_IsCaseInsensitive()
    {
        Size size = SizeFaker.GenerateSize();
        size.Name = "xl";
        await context.Sizes.AddAsync(size);
        await context.SaveChangesAsync();
 
        bool result = await repository.IsNameAlreadyExistsAsync("XL");
        result.Should().BeTrue();
    }
}