using Clothy.CatalogService.DAL.DB;
using Clothy.CatalogService.DAL.Repositories;
using Clothy.CatalogService.Domain.Entities.Catalog;
using Clothy.CatalogService.Domain.Entities.Clothe;
using Clothy.CatalogService.Domain.Entities.Stock;
using Clothy.CatalogService.Domain.QueryParameters;
using Clothy.CatalogService.UnitTests.Helpers;
using Clothy.CatalogService.UnitTests.Helpers.Persistance;
using Clothy.Shared.Helpers;
using FluentAssertions;
using Xunit;

namespace Clothy.CatalogService.UnitTests.Repositories;

public class ClothesStockRepositoryTests : IDisposable
{
    private ClothyCatalogDbContext context;
    private ClothesStockRepository repository;
 
    public ClothesStockRepositoryTests()
    {
        context = DbContextFactory.CreateInMemoryContext();
        repository = new ClothesStockRepository(context);
    }
 
    public void Dispose() => context.Dispose();
 
    private async Task<(ClotheItem clothe, Color color, Size size)> SeedDependenciesAsync()
    {
        ClotheItem clothe = ClotheItemFaker.GenerateClotheItem();
        Color color = ColorFaker.GenerateColor();
        Size size = SizeFaker.GenerateSize();
 
        await context.ClotheItems.AddAsync(clothe);
        await context.Colors.AddAsync(color);
        await context.Sizes.AddAsync(size);
        await context.SaveChangesAsync();
 
        return (clothe, color, size);
    }
    
    [Fact]
    public async Task GetByClotheColorSizeAsync_WhenMatchExists_ReturnsStock()
    {
        (ClotheItem clothe, Color color, Size size) = await SeedDependenciesAsync();
 
        ClothesStock stock = new ClothesStock
        {
            Id = Guid.NewGuid(),
            ClotheId = clothe.Id,
            ColorId = color.Id,
            SizeId = size.Id,
            Quantity = 10
        };
 
        await context.ClothesStocks.AddAsync(stock);
        await context.SaveChangesAsync();
 
        ClothesStock? result = await repository.GetByClotheColorSizeAsync(clothe.Id, color.Id, size.Id);
 
        result.Should().NotBeNull();
        result!.Id.Should().Be(stock.Id);
        result.Quantity.Should().Be(10);
    }
 
    [Fact]
    public async Task GetByClotheColorSizeAsync_WhenNoMatch_ReturnsNull()
    {
        ClothesStock? result = await repository.GetByClotheColorSizeAsync(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
 
        result.Should().BeNull();
    }
 
    [Fact]
    public async Task GetByClotheColorSizeAsync_WhenOnlyPartialMatch_ReturnsNull()
    {
        (ClotheItem clothe, Color color, Size size) = await SeedDependenciesAsync();
 
        ClothesStock stock = new ClothesStock
        {
            Id = Guid.NewGuid(),
            ClotheId = clothe.Id,
            ColorId = color.Id,
            SizeId = size.Id,
            Quantity = 5
        };
 
        await context.ClothesStocks.AddAsync(stock);
        await context.SaveChangesAsync();
 
        ClothesStock? result = await repository.GetByClotheColorSizeAsync(
            clothe.Id, color.Id, Guid.NewGuid());
 
        result.Should().BeNull();
    }
    
    [Fact]
    public async Task GetByIdWithDetailsAsync_WhenExists_ReturnsStockWithIncludes()
    {
        (ClotheItem clothe, Color color, Size size) = await SeedDependenciesAsync();
 
        ClothesStock stock = new ClothesStock
        {
            Id = Guid.NewGuid(),
            ClotheId = clothe.Id,
            ColorId = color.Id,
            SizeId = size.Id,
            Quantity = 7
        };
 
        await context.ClothesStocks.AddAsync(stock);
        await context.SaveChangesAsync();
 
        ClothesStock? result = await repository.GetByIdWithDetailsAsync(stock.Id);
 
        result.Should().NotBeNull();
        result!.Clothe.Should().NotBeNull();
        result.Color.Should().NotBeNull();
        result.Size.Should().NotBeNull();
    }
 
    [Fact]
    public async Task GetByIdWithDetailsAsync_WhenDoesNotExist_ReturnsNull()
    {
        ClothesStock? result = await repository.GetByIdWithDetailsAsync(Guid.NewGuid());
 
        result.Should().BeNull();
    }
 
 
    [Fact]
    public async Task GetTotalQuantityAsync_ReturnsSumOfAllQuantities()
    {
        (ClotheItem clothe, Color color, Size size) = await SeedDependenciesAsync();
 
        ClothesStock stock1 = new ClothesStock
        {
            Id = Guid.NewGuid(),
            ClotheId = clothe.Id,
            ColorId = color.Id,
            SizeId = size.Id,
            Quantity = 30
        };
 
        Size size2 = SizeFaker.GenerateSize();
        await context.Sizes.AddAsync(size2);
        await context.SaveChangesAsync();
 
        ClothesStock stock2 = new ClothesStock
        {
            Id = Guid.NewGuid(),
            ClotheId = clothe.Id,
            ColorId = color.Id,
            SizeId = size2.Id,
            Quantity = 20
        };
 
        await context.ClothesStocks.AddRangeAsync(stock1, stock2);
        await context.SaveChangesAsync();
 
        int total = await repository.GetTotalQuantityAsync();
 
        total.Should().Be(50);
    }
 
    [Fact]
    public async Task GetTotalQuantityAsync_WhenNoStocks_ReturnsZero()
    {
        int total = await repository.GetTotalQuantityAsync();
 
        total.Should().Be(0);
    }
    
    [Fact]
    public async Task GetPagedClothesStockAsync_ReturnsCorrectPageData()
    {
        ClotheItem clothe = ClotheItemFaker.GenerateClotheItem();
        Color color = ColorFaker.GenerateColor();
        await context.ClotheItems.AddAsync(clothe);
        await context.Colors.AddAsync(color);
        await context.SaveChangesAsync();
 
        List<Size> sizes = SizeFaker.GenerateSizes(12);
        await context.Sizes.AddRangeAsync(sizes);
        await context.SaveChangesAsync();
 
        List<ClothesStock> stocks = sizes.Select(s => new ClothesStock
        {
            Id = Guid.NewGuid(),
            ClotheId = clothe.Id,
            ColorId = color.Id,
            SizeId = s.Id,
            Quantity = 5
        }).ToList();
 
        await context.ClothesStocks.AddRangeAsync(stocks);
        await context.SaveChangesAsync();
 
        ClothesStockSpecificationParameters parameters = new ClothesStockSpecificationParameters
        {
            PageNumber = 1,
            PageSize = 10
        };
 
        PagedList<ClothesStock> result = await repository.GetPagedClothesStockAsync(parameters);
 
        result.Items.Count.Should().Be(10);
        result.TotalCount.Should().Be(12);
        result.CurrentPage.Should().Be(1);
    }
}