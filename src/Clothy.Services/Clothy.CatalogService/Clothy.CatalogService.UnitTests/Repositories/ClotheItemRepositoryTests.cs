using Clothy.CatalogService.DAL.DB;
using Clothy.CatalogService.DAL.Repositories;
using Clothy.CatalogService.Domain.Entities.Clothe;
using Clothy.CatalogService.Domain.QueryParameters;
using Clothy.CatalogService.UnitTests.Helpers;
using Clothy.CatalogService.UnitTests.Helpers.Persistance;
using Clothy.Shared.Helpers;
using FluentAssertions;
using Xunit;

namespace Clothy.CatalogService.UnitTests.Repositories;

public class ClotheItemRepositoryTests : IDisposable
{
    private ClothyCatalogDbContext context;
    private ClotheItemRepository repository;
 
    public ClotheItemRepositoryTests()
    {
        context = DbContextFactory.CreateInMemoryContext();
        repository = new ClotheItemRepository(context);
    }
 
    public void Dispose() => context.Dispose();
    
    [Fact]
    public async Task GetMinAndMaxPriceAsync_ReturnsCorrectMinAndMax()
    {
        ClotheItem cheap = ClotheItemFaker.GenerateClotheItem();
        cheap.Price = 50m;
        ClotheItem expensive = ClotheItemFaker.GenerateClotheItem();
        expensive.Price = 500m;
        ClotheItem mid = ClotheItemFaker.GenerateClotheItem();
        mid.Price = 200m;
 
        await context.ClotheItems.AddRangeAsync(cheap, expensive, mid);
        await context.SaveChangesAsync();
 
        (decimal minPrice, decimal maxPrice) = await repository.GetMinAndMaxPriceAsync();
 
        minPrice.Should().Be(50m);
        maxPrice.Should().Be(500m);
    }
 
    [Fact]
    public async Task GetMinAndMaxPriceAsync_WhenSingleItem_ReturnsSameMinAndMax()
    {
        ClotheItem item = ClotheItemFaker.GenerateClotheItem();
        item.Price = 150m;
        await context.ClotheItems.AddAsync(item);
        await context.SaveChangesAsync();
 
        (decimal minPrice, decimal maxPrice) = await repository.GetMinAndMaxPriceAsync();
 
        minPrice.Should().Be(150m);
        maxPrice.Should().Be(150m);
    }
    
    [Fact]
    public async Task IsSlugAlreadyExistsAsync_WhenSlugExists_AndIdIsNull_ReturnsTrue()
    {
        ClotheItem item = ClotheItemFaker.GenerateClotheItem();
        item.Slug = "test-slug";
        await context.ClotheItems.AddAsync(item);
        await context.SaveChangesAsync();
 
        bool result = await repository.IsSlugAlreadyExistsAsync("test-slug");
 
        result.Should().BeTrue();
    }
 
    [Fact]
    public async Task IsSlugAlreadyExistsAsync_WhenSlugDoesNotExist_ReturnsFalse()
    {
        bool result = await repository.IsSlugAlreadyExistsAsync("non-existent-slug");
 
        result.Should().BeFalse();
    }
 
    [Fact]
    public async Task IsSlugAlreadyExistsAsync_WhenSlugBelongsToSameEntity_ReturnsFalse()
    {
        ClotheItem item = ClotheItemFaker.GenerateClotheItem();
        item.Slug = "my-slug";
        await context.ClotheItems.AddAsync(item);
        await context.SaveChangesAsync();
 
        bool result = await repository.IsSlugAlreadyExistsAsync("my-slug", item.Id);
 
        result.Should().BeFalse();
    }
 
    [Fact]
    public async Task IsSlugAlreadyExistsAsync_WhenSlugBelongsToDifferentEntity_ReturnsTrue()
    {
        ClotheItem item1 = ClotheItemFaker.GenerateClotheItem();
        item1.Slug = "taken-slug";
        ClotheItem item2 = ClotheItemFaker.GenerateClotheItem();
        item2.Slug = "other-slug";
        await context.ClotheItems.AddRangeAsync(item1, item2);
        await context.SaveChangesAsync();
 
        bool result = await repository.IsSlugAlreadyExistsAsync("taken-slug", item2.Id);
 
        result.Should().BeTrue();
    }
 
    [Fact]
    public async Task IsSlugAlreadyExistsAsync_IsCaseInsensitive()
    {
        ClotheItem item = ClotheItemFaker.GenerateClotheItem();
        item.Slug = "my-item-slug";
        await context.ClotheItems.AddAsync(item);
        await context.SaveChangesAsync();
 
        bool result = await repository.IsSlugAlreadyExistsAsync("MY-ITEM-SLUG");
 
        result.Should().BeTrue();
    }
    
    [Fact]
    public async Task GetByIdWithDetailsAsync_WhenItemExists_ReturnsWithNavigationProperties()
    {
        ClotheItem item = ClotheItemFaker.GenerateClotheItem();
        await context.ClotheItems.AddAsync(item);
        await context.SaveChangesAsync();
 
        ClotheItem? result = await repository.GetByIdWithDetailsAsync(item.Id);
 
        result.Should().NotBeNull();
        result!.Id.Should().Be(item.Id);
    }
 
    [Fact]
    public async Task GetByIdWithDetailsAsync_WhenItemDoesNotExist_ReturnsNull()
    {
        ClotheItem? result = await repository.GetByIdWithDetailsAsync(Guid.NewGuid());
 
        result.Should().BeNull();
    }
    
    [Fact]
    public async Task GetBySlugWithDetailsAsync_WhenSlugMatches_ReturnsItem()
    {
        ClotheItem item = ClotheItemFaker.GenerateClotheItem();
        item.Slug = "exact-slug";
        await context.ClotheItems.AddAsync(item);
        await context.SaveChangesAsync();
 
        ClotheItem? result = await repository.GetBySlugWithDetailsAsync("exact-slug");
 
        result.Should().NotBeNull();
        result!.Id.Should().Be(item.Id);
    }
 
    [Fact]
    public async Task GetBySlugWithDetailsAsync_IsCaseInsensitive()
    {
        ClotheItem item = ClotheItemFaker.GenerateClotheItem();
        item.Slug = "lower-slug";
        await context.ClotheItems.AddAsync(item);
        await context.SaveChangesAsync();
 
        ClotheItem? result = await repository.GetBySlugWithDetailsAsync("LOWER-SLUG");
 
        result.Should().NotBeNull();
    }
 
    [Fact]
    public async Task GetBySlugWithDetailsAsync_WhenSlugDoesNotExist_ReturnsNull()
    {
        ClotheItem? result = await repository.GetBySlugWithDetailsAsync("does-not-exist");
 
        result.Should().BeNull();
    }
    
    [Fact]
    public async Task GetClotheItemCountByGenderAsync_ReturnsCorrectCounts()
    {
        List<ClotheItem> maleItems = Enumerable.Range(0, 3)
            .Select(_ =>
            {
                ClotheItem item = ClotheItemFaker.GenerateClotheItem();
                item.Gender = Gender.Male;
                return item;
            }).ToList();
 
        List<ClotheItem> femaleItems = Enumerable.Range(0, 2)
            .Select(_ =>
            {
                ClotheItem item = ClotheItemFaker.GenerateClotheItem();
                item.Gender = Gender.Female;
                return item;
            }).ToList();
 
        await context.ClotheItems.AddRangeAsync(maleItems);
        await context.ClotheItems.AddRangeAsync(femaleItems);
        await context.SaveChangesAsync();
 
        (int maleCount, int femaleCount) = await repository.GetClotheItemCountByGenderAsync();
 
        maleCount.Should().Be(3);
        femaleCount.Should().Be(2);
    }
 
    [Fact]
    public async Task GetClotheItemCountByGenderAsync_WhenNoItems_ReturnZeroCounts()
    {
        (int maleCount, int femaleCount) = await repository.GetClotheItemCountByGenderAsync();
 
        maleCount.Should().Be(0);
        femaleCount.Should().Be(0);
    }
    
    [Fact]
    public async Task GetPagedClotheItemsAsync_ReturnsCorrectPageAndCount()
    {
        List<ClotheItem> items = Enumerable.Range(0, 15)
            .Select(i =>
            {
                ClotheItem item = ClotheItemFaker.GenerateClotheItem();
                item.Slug = $"item-slug-{i}";
                return item;
            }).ToList();
 
        await context.ClotheItems.AddRangeAsync(items);
        await context.SaveChangesAsync();
 
        ClotheItemSpecificationParameters parameters = new ClotheItemSpecificationParameters
        {
            PageNumber = 1,
            PageSize = 10
        };
 
        PagedList<ClotheItem> result = await repository.GetPagedClotheItemsAsync(parameters);
 
        result.Should().NotBeNull();
        result.Items.Count.Should().Be(10);
        result.TotalCount.Should().Be(15);
        result.CurrentPage.Should().Be(1);
        result.PageSize.Should().Be(10);
    }
 
    [Fact]
    public async Task GetPagedClotheItemsAsync_SecondPage_ReturnsRemainingItems()
    {
        List<ClotheItem> items = Enumerable.Range(0, 15)
            .Select(i =>
            {
                ClotheItem item = ClotheItemFaker.GenerateClotheItem();
                item.Slug = $"item-slug-{i}";
                return item;
            }).ToList();
 
        await context.ClotheItems.AddRangeAsync(items);
        await context.SaveChangesAsync();
 
        ClotheItemSpecificationParameters parameters = new ClotheItemSpecificationParameters
        {
            PageNumber = 2,
            PageSize = 10
        };
 
        PagedList<ClotheItem> result = await repository.GetPagedClotheItemsAsync(parameters);
 
        result.Items.Count.Should().Be(5);
        result.TotalCount.Should().Be(15);
        result.CurrentPage.Should().Be(2);
    }
}