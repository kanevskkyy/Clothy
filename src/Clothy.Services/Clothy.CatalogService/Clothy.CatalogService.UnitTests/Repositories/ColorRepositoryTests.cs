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

public class ColorRepositoryTests : IDisposable
{
    private ClothyCatalogDbContext context;
    private ColorRepository repository;

    public ColorRepositoryTests()
    {
        context = DbContextFactory.CreateInMemoryContext();
        repository = new ColorRepository(context);
    }

    public void Dispose() => context.Dispose();

    [Fact]
    public async Task IsNameAlreadyExistsAsync_WhenNameExists_ReturnsTrue()
    {
        Color color = ColorFaker.GenerateColor();
        color.Name = "Red";
        await context.Colors.AddAsync(color);
        await context.SaveChangesAsync();

        bool result = await repository.IsNameAlreadyExistsAsync("Red");
        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsNameAlreadyExistsAsync_WhenNameBelongsToSameEntity_ReturnsFalse()
    {
        Color color = ColorFaker.GenerateColor();
        color.Name = "Red";
        await context.Colors.AddAsync(color);
        await context.SaveChangesAsync();

        bool result = await repository.IsNameAlreadyExistsAsync("Red", color.Id);
        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsHexAlreadyExistsAsync_WhenHexExists_ReturnsTrue()
    {
        Color color = ColorFaker.GenerateColor();
        color.HexCode = "#FF0000";
        await context.Colors.AddAsync(color);
        await context.SaveChangesAsync();

        bool result = await repository.IsHexAlreadyExistsAsync("#FF0000");
        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsHexAlreadyExistsAsync_IsCaseInsensitive()
    {
        Color color = ColorFaker.GenerateColor();
        color.HexCode = "#ff0000";
        await context.Colors.AddAsync(color);
        await context.SaveChangesAsync();

        bool result = await repository.IsHexAlreadyExistsAsync("#FF0000");
        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsHexAlreadyExistsAsync_WhenHexBelongsToSameEntity_ReturnsFalse()
    {
        Color color = ColorFaker.GenerateColor();
        color.HexCode = "#FF0000";
        await context.Colors.AddAsync(color);
        await context.SaveChangesAsync();

        bool result = await repository.IsHexAlreadyExistsAsync("#FF0000", color.Id);
        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsSlugAlreadyExistsAsync_WhenSlugExists_ReturnsTrue()
    {
        Color color = ColorFaker.GenerateColor();
        color.Slug = "red";
        await context.Colors.AddAsync(color);
        await context.SaveChangesAsync();

        bool result = await repository.IsSlugAlreadyExistsAsync("red");
        result.Should().BeTrue();
    }
}