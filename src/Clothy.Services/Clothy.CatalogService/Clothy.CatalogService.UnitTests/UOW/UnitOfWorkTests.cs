using Clothy.CatalogService.DAL.DB;
using Clothy.CatalogService.DAL.Repositories;
using Clothy.CatalogService.DAL.UOW;
using Clothy.CatalogService.Domain.Entities.Catalog;
using Clothy.CatalogService.UnitTests.Helpers;
using Clothy.CatalogService.UnitTests.Helpers.Persistance;
using FluentAssertions;
using Xunit;

namespace Clothy.CatalogService.UnitTests.UOW;

public class UnitOfWorkTests : IDisposable
{
    private ClothyCatalogDbContext context;
    private UnitOfWork unitOfWork;
 
    public UnitOfWorkTests()
    {
        context = DbContextFactory.CreateInMemoryContext();
        unitOfWork = BuildUnitOfWork(context);
    }
 
    public void Dispose() => unitOfWork.Dispose();
 
    private static UnitOfWork BuildUnitOfWork(ClothyCatalogDbContext ctx) =>
        new UnitOfWork(
            ctx,
            new BrandRepository(ctx),
            new ClotheItemRepository(ctx),
            new ClothesStockRepository(ctx),
            new CollectionRepository(ctx),
            new ColorRepository(ctx),
            new MaterialRepository(ctx),
            new SizeRepository(ctx),
            new TagRepository(ctx),
            new ClothingTypeRepository(ctx),
            new StockNotificationRepository(ctx),
            new ClothePopularityRepository(ctx));
    
    [Fact]
    public void UnitOfWork_AllRepositoriesAreInitialized()
    {
        unitOfWork.Brands.Should().NotBeNull();
        unitOfWork.ClotheItems.Should().NotBeNull();
        unitOfWork.ClothesStocks.Should().NotBeNull();
        unitOfWork.Collections.Should().NotBeNull();
        unitOfWork.Colors.Should().NotBeNull();
        unitOfWork.Materials.Should().NotBeNull();
        unitOfWork.Sizes.Should().NotBeNull();
        unitOfWork.Tags.Should().NotBeNull();
        unitOfWork.ClothingTypes.Should().NotBeNull();
        unitOfWork.StockNotification.Should().NotBeNull();
        unitOfWork.ClothePopularity.Should().NotBeNull();
    }
    
    [Fact]
    public async Task SaveChangesAsync_WhenEntityAdded_PersistsToDatabase()
    {
        Brand brand = BrandFaker.GenerateBrand();
        await unitOfWork.Brands.AddAsync(brand);
 
        int affected = await unitOfWork.SaveChangesAsync();
 
        affected.Should().Be(1);
        Brand? persisted = await context.Brands.FindAsync(brand.Id);
        persisted.Should().NotBeNull();
    }
 
    [Fact]
    public async Task SaveChangesAsync_WhenEntityDeleted_RemovesFromDatabase()
    {
        Brand brand = BrandFaker.GenerateBrand();
        await context.Brands.AddAsync(brand);
        await context.SaveChangesAsync();
 
        Brand? loaded = await unitOfWork.Brands.GetByIdAsync(brand.Id);
        unitOfWork.Brands.Delete(loaded!);
 
        await unitOfWork.SaveChangesAsync();
 
        Brand? deleted = await context.Brands.FindAsync(brand.Id);
        deleted.Should().BeNull();
    }
 
    [Fact]
    public async Task SaveChangesAsync_WhenEntityUpdated_ReflectsChange()
    {
        Brand brand = BrandFaker.GenerateBrand();
        await context.Brands.AddAsync(brand);
        await context.SaveChangesAsync();
 
        brand.Name = "Updated Name";
        unitOfWork.Brands.Update(brand);
 
        await unitOfWork.SaveChangesAsync();
 
        Brand? updated = await context.Brands.FindAsync(brand.Id);
        updated!.Name.Should().Be("Updated Name");
    }
 
    [Fact]
    public async Task SaveChangesAsync_WithMultipleEntities_SavesAll()
    {
        Brand brand1 = BrandFaker.GenerateBrand();
        Brand brand2 = BrandFaker.GenerateBrand();
        await unitOfWork.Brands.AddAsync(brand1);
        await unitOfWork.Brands.AddAsync(brand2);
 
        int affected = await unitOfWork.SaveChangesAsync();
 
        affected.Should().Be(2);
    }
}