using Clothy.CatalogService.DAL.DB;
using Clothy.CatalogService.DAL.Repositories;
using Clothy.CatalogService.Domain.Entities.Clothe;
using Clothy.CatalogService.UnitTests.Helpers;
using Clothy.CatalogService.UnitTests.Helpers.Persistance;
using FluentAssertions;
using Xunit;

namespace Clothy.CatalogService.UnitTests.Repositories;

public class ClothePopularityRepositoryTests : IDisposable
{
    private ClothyCatalogDbContext context;
    private ClothePopularityRepository repository;
 
    public ClothePopularityRepositoryTests()
    {
        context = DbContextFactory.CreateInMemoryContext();
        repository = new ClothePopularityRepository(context);
    }
 
    public void Dispose() => context.Dispose();
 
    private async Task<(ClotheItem clothe, ClothePopularity popularity)> SeedPopularityAsync(int soldCount = 10)
    {
        ClotheItem clothe = ClotheItemFaker.GenerateClotheItem();
        await context.ClotheItems.AddAsync(clothe);
        await context.SaveChangesAsync();
 
        ClothePopularity popularity = new ClothePopularity
        {
            Id = Guid.NewGuid(),
            ClotheId = clothe.Id,
            SoldCount = soldCount
        };
        await context.ClothePopularities.AddAsync(popularity);
        await context.SaveChangesAsync();
 
        return (clothe, popularity);
    }
    
    [Fact]
    public async Task GetClothePopularityByClotheIdAsync_WhenExists_ReturnsPopularity()
    {
        (ClotheItem clothe, ClothePopularity popularity) = await SeedPopularityAsync(50);
 
        ClothePopularity? result = await repository.GetClothePopularityByClotheIdAsync(clothe.Id);
 
        result.Should().NotBeNull();
        result!.Id.Should().Be(popularity.Id);
        result.SoldCount.Should().Be(50);
    }
 
    [Fact]
    public async Task GetClothePopularityByClotheIdAsync_WhenDoesNotExist_ReturnsNull()
    {
        ClothePopularity? result = await repository.GetClothePopularityByClotheIdAsync(Guid.NewGuid());
 
        result.Should().BeNull();
    }
    
    [Fact]
    public async Task GetTop8MostPopularAsync_ReturnsMaxEightItems()
    {
        for (int i = 0; i < 10; i++)
        {
            await SeedPopularityAsync(soldCount: 100 - i * 5);
        }
 
        List<ClotheItem> result = await repository.GetTop8MostPopularAsync();
 
        result.Should().HaveCount(8);
    }
 
    [Fact]
    public async Task GetTop8MostPopularAsync_ReturnsOrderedByDescendingSoldCount()
    {
        (ClotheItem low, _) = await SeedPopularityAsync(soldCount: 10);
        (ClotheItem high, _) = await SeedPopularityAsync(soldCount: 200);
        (ClotheItem mid, _) = await SeedPopularityAsync(soldCount: 100);
 
        List<ClotheItem> result = await repository.GetTop8MostPopularAsync();
 
        result.First().Id.Should().Be(high.Id);
        result.Last().Id.Should().Be(low.Id);
    }
 
    [Fact]
    public async Task GetTop8MostPopularAsync_WhenFewerThan8_ReturnsAll()
    {
        await SeedPopularityAsync(10);
        await SeedPopularityAsync(20);
        await SeedPopularityAsync(30);
 
        List<ClotheItem> result = await repository.GetTop8MostPopularAsync();
 
        result.Should().HaveCount(3);
    }
 
    [Fact]
    public async Task GetTop8MostPopularAsync_WhenNoItems_ReturnsEmpty()
    {
        List<ClotheItem> result = await repository.GetTop8MostPopularAsync();
 
        result.Should().BeEmpty();
    }
}