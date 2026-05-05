using Clothy.CatalogService.DAL.DB;
using Clothy.CatalogService.DAL.Repositories;
using Clothy.CatalogService.Domain.Entities.Catalog;
using Clothy.CatalogService.Domain.Entities.Clothe;
using Clothy.CatalogService.Domain.Entities.Stock;
using Clothy.CatalogService.UnitTests.Helpers;
using Clothy.CatalogService.UnitTests.Helpers.Persistance;
using FluentAssertions;
using Xunit;

namespace Clothy.CatalogService.UnitTests.Repositories;

public class StockNotificationRepositoryTests : IDisposable
{
    private ClothyCatalogDbContext context;
    private StockNotificationRepository repository;
 
    public StockNotificationRepositoryTests()
    {
        context = DbContextFactory.CreateInMemoryContext();
        repository = new StockNotificationRepository(context);
    }
 
    public void Dispose() => context.Dispose();
 
    private async Task<ClothesStock> SeedStockAsync()
    {
        ClotheItem clothe = ClotheItemFaker.GenerateClotheItem();
        Color color = ColorFaker.GenerateColor();
        Size size = SizeFaker.GenerateSize();
        await context.ClotheItems.AddAsync(clothe);
        await context.Colors.AddAsync(color);
        await context.Sizes.AddAsync(size);
        await context.SaveChangesAsync();
 
        ClothesStock stock = new ClothesStock
        {
            Id = Guid.NewGuid(),
            ClotheId = clothe.Id,
            ColorId = color.Id,
            SizeId = size.Id,
            Quantity = 0
        };
        await context.ClothesStocks.AddAsync(stock);
        await context.SaveChangesAsync();
        return stock;
    }
 
    [Fact]
    public async Task HasUserAlreadySubscribeInStockId_WhenUserNotSubscribed_ReturnsFalse()
    {
        bool result = await repository.HasUserAlreadySubscribeInStockId(Guid.NewGuid(), Guid.NewGuid());
 
        result.Should().BeFalse();
    }
 
    [Fact]
    public async Task GetAllSubscribersByStockId_WhenNoSubscribers_ReturnsEmpty()
    {
        List<StockNotification> result = await repository.GetAllSubscribersByStockId(Guid.NewGuid());
 
        result.Should().BeEmpty();
    }
}