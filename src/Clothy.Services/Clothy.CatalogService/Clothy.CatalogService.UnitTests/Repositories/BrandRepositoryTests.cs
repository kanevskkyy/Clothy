using Clothy.CatalogService.DAL.DB;
using Clothy.CatalogService.DAL.Repositories;
using Clothy.CatalogService.Domain.Entities.Catalog;
using Clothy.CatalogService.Domain.Entities.Clothe;
using Clothy.CatalogService.UnitTests.Helpers;
using Clothy.CatalogService.UnitTests.Helpers.Persistance;
using FluentAssertions;
using Xunit;

namespace Clothy.CatalogService.UnitTests.Repositories;

public class BrandRepositoryTests : IDisposable
{
    private ClothyCatalogDbContext context;
    private BrandRepository repository;
 
    public BrandRepositoryTests()
    {
        context = DbContextFactory.CreateInMemoryContext();
        repository = new BrandRepository(context);
    }
 
    public void Dispose() => context.Dispose();
 
    [Fact]
    public async Task IsNameAlreadyExistsAsync_WhenNameExists_AndIdIsNull_ReturnsTrue()
    {
        Brand brand = BrandFaker.GenerateBrand();
        await context.Brands.AddAsync(brand);
        await context.SaveChangesAsync();
 
        bool result = await repository.IsNameAlreadyExistsAsync(brand.Name!);
 
        result.Should().BeTrue();
    }
 
    [Fact]
    public async Task IsNameAlreadyExistsAsync_WhenNameDoesNotExist_AndIdIsNull_ReturnsFalse()
    {
        bool result = await repository.IsNameAlreadyExistsAsync("NonExistentBrand");
 
        result.Should().BeFalse();
    }
 
    [Fact]
    public async Task IsNameAlreadyExistsAsync_WhenNameExistsForDifferentEntity_AndIdProvided_ReturnsTrue()
    {
        Brand existing = BrandFaker.GenerateBrand();
        Brand other = BrandFaker.GenerateBrand();
        await context.Brands.AddRangeAsync(existing, other);
        await context.SaveChangesAsync();
 
        bool result = await repository.IsNameAlreadyExistsAsync(existing.Name!, other.Id);
 
        result.Should().BeTrue();
    }
 
    [Fact]
    public async Task IsNameAlreadyExistsAsync_WhenNameBelongsToSameEntity_AndIdProvided_ReturnsFalse()
    {
        Brand brand = BrandFaker.GenerateBrand();
        await context.Brands.AddAsync(brand);
        await context.SaveChangesAsync();
 
        bool result = await repository.IsNameAlreadyExistsAsync(brand.Name!, brand.Id);
 
        result.Should().BeFalse();
    }
 
    [Fact]
    public async Task IsNameAlreadyExistsAsync_IsCaseInsensitive()
    {
        Brand brand = BrandFaker.GenerateBrand();
        brand.Name = "Nike";
        await context.Brands.AddAsync(brand);
        await context.SaveChangesAsync();
 
        bool result = await repository.IsNameAlreadyExistsAsync("NIKE");
 
        result.Should().BeTrue();
    }
 
    [Fact]
    public async Task IsSlugAlreadyExistsAsync_WhenSlugExists_AndIdIsNull_ReturnsTrue()
    {
        Brand brand = BrandFaker.GenerateBrand();
        await context.Brands.AddAsync(brand);
        await context.SaveChangesAsync();
 
        bool result = await repository.IsSlugAlreadyExistsAsync(brand.Slug!);
 
        result.Should().BeTrue();
    }
 
    [Fact]
    public async Task IsSlugAlreadyExistsAsync_WhenSlugDoesNotExist_ReturnsTrue()
    {
        bool result = await repository.IsSlugAlreadyExistsAsync("non-existent-slug");
 
        result.Should().BeFalse();
    }
 
    [Fact]
    public async Task IsSlugAlreadyExistsAsync_WhenSlugExistsForDifferentEntity_AndIdProvided_ReturnsTrue()
    {
        Brand existing = BrandFaker.GenerateBrand();
        Brand other = BrandFaker.GenerateBrand();
        await context.Brands.AddRangeAsync(existing, other);
        await context.SaveChangesAsync();
 
        bool result = await repository.IsSlugAlreadyExistsAsync(existing.Slug!, other.Id);
 
        result.Should().BeTrue();
    }
 
    [Fact]
    public async Task IsSlugAlreadyExistsAsync_WhenSlugBelongsToSameEntity_AndIdProvided_ReturnsFalse()
    {
        Brand brand = BrandFaker.GenerateBrand();
        await context.Brands.AddAsync(brand);
        await context.SaveChangesAsync();
 
        bool result = await repository.IsSlugAlreadyExistsAsync(brand.Slug!, brand.Id);
 
        result.Should().BeFalse();
    }
 
    [Fact]
    public async Task GetBrandsWithStockCountAsync_WhenNoBrandsExist_ReturnsEmptyDictionary()
    {
        Dictionary<Brand, int> result = await repository.GetBrandsWithStockCountAsync();
 
        result.Should().BeEmpty();
    }
}