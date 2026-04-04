using Clothy.CatalogService.DAL.DB;
using Clothy.CatalogService.DAL.Repositories;
using Clothy.CatalogService.Domain.Entities.Catalog;
using Clothy.CatalogService.Domain.Entities.Clothe;
using Clothy.CatalogService.UnitTests.Helpers;
using Clothy.CatalogService.UnitTests.Helpers.Persistance;
using FluentAssertions;
using Xunit;

namespace Clothy.CatalogService.UnitTests.Repositories;

public class MaterialRepositoryTests : IDisposable
{
    private ClothyCatalogDbContext context;
    private MaterialRepository repository;
 
    public MaterialRepositoryTests()
    {
        context = DbContextFactory.CreateInMemoryContext();
        repository = new MaterialRepository(context);
    }
 
    public void Dispose() => context.Dispose();
 
    [Fact]
    public async Task IsNameAlreadyExistsAsync_WhenNameExists_ReturnsTrue()
    {
        Material material = MaterialFaker.GenerateMaterial();
        material.Name = "Cotton";
        await context.Materials.AddAsync(material);
        await context.SaveChangesAsync();
 
        bool result = await repository.IsNameAlreadyExistsAsync("Cotton");
        result.Should().BeTrue();
    }
 
    [Fact]
    public async Task IsNameAlreadyExistsAsync_WhenNameBelongsToSameEntity_ReturnsFalse()
    {
        Material material = MaterialFaker.GenerateMaterial();
        material.Name = "Cotton";
        await context.Materials.AddAsync(material);
        await context.SaveChangesAsync();
 
        bool result = await repository.IsNameAlreadyExistsAsync("Cotton", material.Id);
        result.Should().BeFalse();
    }
 
    [Fact]
    public async Task AreAllExistAsync_WhenAllIdsExist_ReturnsTrue()
    {
        List<Material> materials = MaterialFaker.GenerateMaterials(3);
        await context.Materials.AddRangeAsync(materials);
        await context.SaveChangesAsync();
 
        bool result = await repository.AreAllExistAsync(materials.Select(m => m.Id));
        result.Should().BeTrue();
    }
 
    [Fact]
    public async Task AreAllExistAsync_WhenSomeIdsDoNotExist_ReturnsFalse()
    {
        List<Material> materials = MaterialFaker.GenerateMaterials(2);
        await context.Materials.AddRangeAsync(materials);
        await context.SaveChangesAsync();
 
        List<Guid> ids = materials.Select(m => m.Id).Append(Guid.NewGuid()).ToList();
 
        bool result = await repository.AreAllExistAsync(ids);
        result.Should().BeFalse();
    }
 
    [Fact]
    public async Task AreAllExistAsync_WhenEmptyCollection_ReturnsTrue()
    {
        bool result = await repository.AreAllExistAsync(Enumerable.Empty<Guid>());
        result.Should().BeTrue();
    }
 
    [Fact]
    public async Task IsSlugAlreadyExistsAsync_WhenSlugExists_ReturnsTrue()
    {
        Material material = MaterialFaker.GenerateMaterial();
        material.Slug = "cotton";
        await context.Materials.AddAsync(material);
        await context.SaveChangesAsync();
 
        bool result = await repository.IsSlugAlreadyExistsAsync("cotton");
        result.Should().BeTrue();
    }
}