using System.Net;
using System.Net.Http.Json;
using Clothy.CatalogService.BLL.DTOs.ClotheDTOs;
using Clothy.CatalogService.DAL.DB;
using Clothy.CatalogService.Domain.Entities.Catalog;
using Clothy.CatalogService.Domain.Entities.Clothe;
using Clothy.CatalogService.Domain.Entities.Stock;
using Clothy.CatalogService.IntegrationTests.Infrastructure;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Clothy.CatalogService.IntegrationTests.Controllers;

[Collection("CatalogService")]
public class ClotheControllerTests : IAsyncLifetime
{
    private HttpClient client;
    private CatalogServiceWebApplicationFactory factory;

    public ClotheControllerTests(CatalogServiceWebApplicationFactory factory)
    {
        this.factory = factory;
        client = factory.CreateClient();

        factory.ImageServiceMock
            .Setup(x => x.UploadAsync(It.IsAny<IFormFile>(), It.IsAny<string>(), false))
            .ReturnsAsync("https://fake-image.url/photo.jpg");

        factory.ImageServiceMock
            .Setup(x => x.DeleteImageAsync(It.IsAny<string>()))
            .Returns(Task.CompletedTask);
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        using var scope = factory.Services.CreateScope();
        ClothyCatalogDbContext db = scope.ServiceProvider.GetRequiredService<ClothyCatalogDbContext>();

        db.ClotheTags.RemoveRange(db.ClotheTags);
        db.ClotheMaterials.RemoveRange(db.ClotheMaterials);
        db.PhotoClothes.RemoveRange(db.PhotoClothes);
        db.StockNotifications.RemoveRange(db.StockNotifications);
        db.ClothesStocks.RemoveRange(db.ClothesStocks);
        db.ClothePopularities.RemoveRange(db.ClothePopularities);
        db.ClotheItems.RemoveRange(db.ClotheItems);
        db.Brands.RemoveRange(db.Brands);
        db.ClothingTypes.RemoveRange(db.ClothingTypes);
        db.Collections.RemoveRange(db.Collections);
        db.Colors.RemoveRange(db.Colors);
        db.Sizes.RemoveRange(db.Sizes);
        db.Materials.RemoveRange(db.Materials);
        db.Tags.RemoveRange(db.Tags);
        await db.SaveChangesAsync();
    }

    [Fact]
    public async Task GetPaged_ShouldReturnOk_WithoutAuth()
    {
        HttpResponseMessage response = await client.GetAsync("/api/clothes");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetBySlug_ShouldReturnNotFound_WhenDoesNotExist()
    {
        HttpResponseMessage response = await client.GetAsync("/api/clothes/non-existent-slug");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Create_AsAdmin_ShouldReturnCreated()
    {
        Dependencies deps = await SeedDependenciesAsync();
        client.AddAuthorizationHeader(Guid.NewGuid(), new[] { "Admin" });
        MultipartFormDataContent form = BuildCreateForm(deps);

        HttpResponseMessage response = await client.PostAsync("/api/clothes", form);
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task Create_ShouldReturnConflict_WhenSlugAlreadyExists()
    {
        SeedResult seed = await SeedClotheAsync();
        client.AddAuthorizationHeader(Guid.NewGuid(), new[] { "Admin" });
        MultipartFormDataContent form = BuildCreateForm(seed.Dependencies, slug: seed.Slug);

        HttpResponseMessage response = await client.PostAsync("/api/clothes", form);
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Update_ShouldReturnUnauthorized_WhenNoToken()
    {
        SeedResult seed = await SeedClotheAsync();
        MultipartFormDataContent form = BuildUpdateForm(seed.Dependencies, name: "Updated", slug: $"updated-{seed.U}");

        HttpResponseMessage response = await client.PutAsync($"/api/clothes/{seed.ClotheId}", form);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Update_AsUser_ShouldReturnForbidden()
    {
        SeedResult seed = await SeedClotheAsync();
        client.AddAuthorizationHeader(Guid.NewGuid(), new[] { "User" });
        MultipartFormDataContent form = BuildUpdateForm(seed.Dependencies, name: "Updated", slug: $"updated-{seed.U}");

        HttpResponseMessage response = await client.PutAsync($"/api/clothes/{seed.ClotheId}", form);
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Update_AsAdmin_ShouldReturnOk()
    {
        SeedResult seed = await SeedClotheAsync();
        client.AddAuthorizationHeader(Guid.NewGuid(), new[] { "Admin" });
        MultipartFormDataContent form =
            BuildUpdateForm(seed.Dependencies, name: "Admin Updated", slug: $"admin-updated-{seed.U}");

        HttpResponseMessage response = await client.PutAsync($"/api/clothes/{seed.ClotheId}", form);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Update_ShouldReturnNotFound_WhenDoesNotExist()
    {
        Dependencies deps = await SeedDependenciesAsync();
        client.AddAuthorizationHeader(Guid.NewGuid(), new[] { "Admin" });
        MultipartFormDataContent form = BuildUpdateForm(deps, name: "Ghost", slug: $"ghost-{deps.U}");

        HttpResponseMessage response = await client.PutAsync($"/api/clothes/{Guid.NewGuid()}", form);
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Delete_ShouldReturnUnauthorized_WhenNoToken()
    {
        SeedResult seed = await SeedClotheAsync();
        HttpResponseMessage response = await client.DeleteAsync($"/api/clothes/{seed.ClotheId}");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Delete_AsUser_ShouldReturnForbidden()
    {
        SeedResult seed = await SeedClotheAsync();
        client.AddAuthorizationHeader(Guid.NewGuid(), new[] { "User" });

        HttpResponseMessage response = await client.DeleteAsync($"/api/clothes/{seed.ClotheId}");
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Delete_AsManager_ShouldReturnForbidden()
    {
        SeedResult seed = await SeedClotheAsync();
        client.AddAuthorizationHeader(Guid.NewGuid(), new[] { "Manager" });

        HttpResponseMessage response = await client.DeleteAsync($"/api/clothes/{seed.ClotheId}");
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Delete_AsAdmin_ShouldReturnNoContent()
    {
        SeedResult seed = await SeedClotheAsync();
        client.AddAuthorizationHeader(Guid.NewGuid(), new[] { "Admin" });

        HttpResponseMessage response = await client.DeleteAsync($"/api/clothes/{seed.ClotheId}");
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Delete_ShouldReturnNotFound_WhenDoesNotExist()
    {
        client.AddAuthorizationHeader(Guid.NewGuid(), new[] { "Admin" });
        HttpResponseMessage response = await client.DeleteAsync($"/api/clothes/{Guid.NewGuid()}");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    private record Dependencies(
        string U,
        Guid BrandId,
        Guid ClothingTypeId,
        Guid CollectionId,
        Guid ColorId,
        Guid SizeId,
        Guid MaterialId,
        Guid TagId);

    private record SeedResult(
        string U,
        string Slug,
        Guid ClotheId,
        Dependencies Dependencies);

    private async Task<Dependencies> SeedDependenciesAsync()
    {
        using var scope = factory.Services.CreateScope();
        ClothyCatalogDbContext db = scope.ServiceProvider.GetRequiredService<ClothyCatalogDbContext>();

        string u = Guid.NewGuid().ToString("N")[..8];

        Brand brand = new Brand
        {
            Id = Guid.NewGuid(),
            Name = $"Brand-{u}",
            Slug = $"brand-{u}"
        };
        ClothingType type = new ClothingType
        {
            Id = Guid.NewGuid(),
            Name = $"Type-{u}",
            Slug = $"type-{u}"
        };
        Collection collection = new Collection
        {
            Id = Guid.NewGuid(),
            Name = $"Col-{u}",
            Slug = $"col-{u}",
            CreatedAt = DateTime.UtcNow
        };
        Color color = new Color
        {
            Id = Guid.NewGuid(),
            Name = $"Black-{u}",
            Slug = $"black-{u}",
            HexCode = "#000000"
        };
        Size size = new Size
        {
            Id = Guid.NewGuid(),
            Name = $"M-{u}"
        };
        Material material = new Material
        {
            Id = Guid.NewGuid(),
            Name = $"Cotton-{u}", Slug = $"cotton-{u}"
        };
        Tag tag = new Tag
        {
            Id = Guid.NewGuid(),
            Name = $"Tag-{u}", Slug = $"tag-{u}"
        };

        db.Brands.Add(brand);
        db.ClothingTypes.Add(type);
        db.Collections.Add(collection);
        db.Colors.Add(color);
        db.Sizes.Add(size);
        db.Materials.Add(material);
        db.Tags.Add(tag);
        await db.SaveChangesAsync();

        return new Dependencies(u, brand.Id, type.Id, collection.Id, color.Id, size.Id, material.Id, tag.Id);
    }

    private async Task<SeedResult> SeedClotheAsync()
    {
        Dependencies deps = await SeedDependenciesAsync();

        using var scope = factory.Services.CreateScope();
        ClothyCatalogDbContext db = scope.ServiceProvider.GetRequiredService<ClothyCatalogDbContext>();

        string slug = $"clothe-{deps.U}";

        ClotheItem clothe = new ClotheItem
        {
            Id = Guid.NewGuid(),
            Name = $"Clothe-{deps.U}",
            Slug = slug,
            Description = "Test description",
            Price = 100m,
            Gender = Gender.Male,
            BrandId = deps.BrandId,
            ClothingTypeId = deps.ClothingTypeId,
            CollectionId = deps.CollectionId,
            CreatedAt = DateTime.UtcNow,
            Photos = new List<PhotoClothes>
            {
                new PhotoClothes
                {
                    Id = Guid.NewGuid(),
                    PhotoURL = "https://fake-image.url/photo.jpg",
                    ColorId = deps.ColorId,
                    IsMain = true
                }
            },
            ClotheMaterials = new List<ClotheMaterial>
            {
                new ClotheMaterial
                {
                    MaterialId = deps.MaterialId,
                    Percentage = 100
                }
            },
            ClotheTags = new List<ClotheTag>
            {
                new ClotheTag
                {
                    TagId = deps.TagId
                }
            },
            Stocks = new List<ClothesStock>
            {
                new ClothesStock
                {
                    Id = Guid.NewGuid(),
                    SizeId = deps.SizeId,
                    ColorId = deps.ColorId,
                    Quantity = 0
                }
            }
        };

        db.ClotheItems.Add(clothe);
        await db.SaveChangesAsync();

        return new SeedResult(deps.U, slug, clothe.Id, deps);
    }

    private MultipartFormDataContent BuildCreateForm(Dependencies deps, string? slug = null)
    {
        string u = Guid.NewGuid().ToString("N")[..8];
        slug ??= $"clothe-{u}";

        byte[] fakeImage = new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 };
        ByteArrayContent imageContent = new ByteArrayContent(fakeImage);
        imageContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");

        MultipartFormDataContent form = new MultipartFormDataContent
        {
            { new StringContent($"Clothe-{u}"), "Name" },
            { new StringContent(slug), "Slug" },
            { new StringContent("Test description"), "Description" },
            { new StringContent("100"), "Price" },
            { new StringContent("1"), "Gender" },
            { new StringContent(deps.BrandId.ToString()), "BrandId" },
            { new StringContent(deps.ClothingTypeId.ToString()), "ClothingTypeId" },
            { new StringContent(deps.CollectionId.ToString()), "CollectionId" },
            { new StringContent(deps.TagId.ToString()), "TagIds[0]" },
            { new StringContent($"[{{\"MaterialId\":\"{deps.MaterialId}\",\"Percentage\":100}}]"), "Materials" },
            { imageContent, "AdditionalPhotos[0].Photo", "photo.jpg" },
            { new StringContent(deps.ColorId.ToString()), "AdditionalPhotos[0].ColorId" },
            { new StringContent("true"), "AdditionalPhotos[0].IsMain" },
        };

        return form;
    }

    private MultipartFormDataContent BuildUpdateForm(Dependencies deps, string name, string slug, decimal price = 100m)
    {
        MultipartFormDataContent form = new MultipartFormDataContent
        {
            { new StringContent(name), "Name" },
            { new StringContent(slug), "Slug" },
            { new StringContent("Updated description"), "Description" },
            { new StringContent(price.ToString(System.Globalization.CultureInfo.InvariantCulture)), "Price" },
            { new StringContent("1"), "Gender" },
            { new StringContent(deps.BrandId.ToString()), "BrandId" },
            { new StringContent(deps.ClothingTypeId.ToString()), "ClothingTypeId" },
            { new StringContent(deps.CollectionId.ToString()), "CollectionId" },
        };

        return form;
    }
}