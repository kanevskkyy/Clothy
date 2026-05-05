using Clothy.CatalogService.DAL.DB;
using Clothy.CatalogService.DAL.Repositories;
using Clothy.CatalogService.Domain.Entities.Catalog;
using Clothy.CatalogService.UnitTests.Helpers;
using Clothy.CatalogService.UnitTests.Helpers.Persistance;
using FluentAssertions;
using Xunit;

namespace Clothy.CatalogService.UnitTests.Repositories;

public class GenericRepositoryTests : IDisposable
{
    private ClothyCatalogDbContext context;
    private GenericRepository<Brand> genericRepository;
 
    public GenericRepositoryTests()
    {
        context = DbContextFactory.CreateInMemoryContext();
        genericRepository = new BrandRepository(context);
    }
 
    public void Dispose() => context.Dispose();
 
 
    [Fact]
    public async Task GetAllAsync_WhenEntitiesExist_ReturnsAll()
    {
        List<Brand> brands = BrandFaker.GenerateBrands(3);
        await context.Set<Brand>().AddRangeAsync(brands);
        await context.SaveChangesAsync();
 
        IReadOnlyList<Brand> result = await genericRepository.GetAllAsync(CancellationToken.None);
 
        result.Should().HaveCount(3);
        result.Select(b => b.Id).Should().BeEquivalentTo(brands.Select(b => b.Id));
    }
 
    [Fact]
    public async Task GetAllAsync_WhenNoEntities_ReturnsEmpty()
    {
        IReadOnlyList<Brand> result = await genericRepository.GetAllAsync(CancellationToken.None);
 
        result.Should().BeEmpty();
    }
    
    [Fact]
    public async Task GetByIdAsync_WhenEntityExists_ReturnsEntity()
    {
        Brand brand = BrandFaker.GenerateBrand();
        await context.Set<Brand>().AddAsync(brand);
        await context.SaveChangesAsync();
 
        Brand? result = await genericRepository.GetByIdAsync(brand.Id);
 
        result.Should().NotBeNull();
        result!.Id.Should().Be(brand.Id);
        result.Name.Should().Be(brand.Name);
    }
 
    [Fact]
    public async Task GetByIdAsync_WhenEntityDoesNotExist_ReturnsNull()
    {
        Brand? result = await genericRepository.GetByIdAsync(Guid.NewGuid());
 
        result.Should().BeNull();
    }
 
    [Fact]
    public async Task AddAsync_ThenSaveChanges_PersistsEntity()
    {
        Brand brand = BrandFaker.GenerateBrand();
 
        await genericRepository.AddAsync(brand);
        await context.SaveChangesAsync();
 
        Brand? persisted = await context.Set<Brand>().FindAsync(brand.Id);
        persisted.Should().NotBeNull();
        persisted!.Name.Should().Be(brand.Name);
    }
    
    [Fact]
    public async Task Update_ThenSaveChanges_UpdatesEntity()
    {
        Brand brand = BrandFaker.GenerateBrand();
        await context.Set<Brand>().AddAsync(brand);
        await context.SaveChangesAsync();
 
        string newName = "Updated Brand Name";
        brand.Name = newName;
 
        genericRepository.Update(brand);
        await context.SaveChangesAsync();
 
        Brand? updated = await context.Set<Brand>().FindAsync(brand.Id);
        updated!.Name.Should().Be(newName);
    }
    
    [Fact]
    public async Task Delete_ThenSaveChanges_RemovesEntity()
    {
        Brand brand = BrandFaker.GenerateBrand();
        await context.Set<Brand>().AddAsync(brand);
        await context.SaveChangesAsync();
 
        genericRepository.Delete(brand);
        await context.SaveChangesAsync();
 
        Brand? deleted = await context.Set<Brand>().FindAsync(brand.Id);
        deleted.Should().BeNull();
    }
 
    [Fact]
    public void ApplySpecification_WithNullSpec_ThrowsArgumentNullException()
    {
        Action act = () => genericRepository.ApplySpecification(null!);
 
        act.Should().Throw<ArgumentNullException>();
    }
}