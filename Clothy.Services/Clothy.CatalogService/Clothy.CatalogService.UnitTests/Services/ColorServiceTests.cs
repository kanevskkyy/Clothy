using AutoMapper;
using Clothy.Aggregator.Aggregate.RedisCache;
using Clothy.CatalogService.BLL.DTOs.ColorDTOs;
using Clothy.CatalogService.BLL.Services;
using Clothy.CatalogService.DAL.Interfaces;
using Clothy.CatalogService.DAL.UOW;
using Clothy.CatalogService.Domain.Entities.Catalog;
using Clothy.CatalogService.Domain.Entities.Clothe;
using Clothy.CatalogService.UnitTests.Helpers;
using Clothy.Shared.Cache.Interfaces;
using Clothy.Shared.Helpers.Exceptions;
using FluentAssertions;
using Moq;
using Xunit;

namespace Clothy.CatalogService.UnitTests.Services;

public class ColorServiceTests
{
    private Mock<IUnitOfWork> unitOfWorkMock;
    private Mock<IColorRepository> colorRepoMock;
    private Mock<IMapper> mapperMock;
    private Mock<IFilterCacheInvalidationService> filterCacheInvalidationServiceMock;
    private Mock<IEntityCacheInvalidationService<ClotheItem>> clotheItemInvalidationServiceMock;

    private ColorService colorService;

    public ColorServiceTests()
    {
        unitOfWorkMock = new Mock<IUnitOfWork>();
        colorRepoMock = new Mock<IColorRepository>();
        mapperMock = new Mock<IMapper>();
        filterCacheInvalidationServiceMock = new Mock<IFilterCacheInvalidationService>();
        clotheItemInvalidationServiceMock = new Mock<IEntityCacheInvalidationService<ClotheItem>>();

        unitOfWorkMock
            .Setup(u => u.Colors)
            .Returns(colorRepoMock.Object);

        colorService = new ColorService(
            unitOfWorkMock.Object,
            mapperMock.Object,
            filterCacheInvalidationServiceMock.Object,
            clotheItemInvalidationServiceMock.Object);
    }

    [Fact]
    public async Task GetAllAsync_WhenColorsExist_ReturnsListOfColorReadDTOs()
    {
        List<Color> colors = ColorFaker.GenerateColors(3);
        List<ColorReadDTO> dtos = colors.Select(ColorFaker.GenerateReadDTOFromEntity).ToList();

        colorRepoMock.Setup(r => r.GetAllAsync(default)).ReturnsAsync(colors);
        mapperMock.Setup(m => m.Map<List<ColorReadDTO>>(colors)).Returns(dtos);

        List<ColorReadDTO> result = await colorService.GetAllAsync();

        result.Should().NotBeNull();
        result.Should().HaveCount(3);
        result.Should().BeEquivalentTo(dtos);
    }

    [Fact]
    public async Task GetAllAsync_WhenNoColorsExist_ReturnsEmptyList()
    {
        colorRepoMock.Setup(r => r.GetAllAsync(default)).ReturnsAsync(new List<Color>());
        mapperMock.Setup(m => m.Map<List<ColorReadDTO>>(It.IsAny<List<Color>>())).Returns(new List<ColorReadDTO>());

        List<ColorReadDTO> result = await colorService.GetAllAsync();

        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetByIdAsync_WhenColorExists_ReturnsColorReadDTO()
    {
        Color color = ColorFaker.GenerateColor();
        ColorReadDTO dto = ColorFaker.GenerateReadDTOFromEntity(color);

        colorRepoMock.Setup(r => r.GetByIdAsync(color.Id, default)).ReturnsAsync(color);
        mapperMock.Setup(m => m.Map<ColorReadDTO>(color)).Returns(dto);

        ColorReadDTO result = await colorService.GetByIdAsync(color.Id);

        result.Should().NotBeNull();
        result.Id.Should().Be(color.Id);
        result.Name.Should().Be(color.Name);
        result.HexCode.Should().Be(color.HexCode);
    }

    [Fact]
    public async Task GetByIdAsync_WhenColorNotFound_ThrowsNotFoundException()
    {
        Guid id = Guid.NewGuid();

        colorRepoMock.Setup(r => r.GetByIdAsync(id, default)).ReturnsAsync((Color?)null);

        Func<Task> act = async () => await colorService.GetByIdAsync(id);

        await act.Should().ThrowAsync<NotFoundException>().WithMessage($"*{id}*");
    }

    [Fact]
    public async Task CreateAsync_WhenValidData_ReturnsColorReadDTO()
    {
        ColorCreateDTO createDTO = ColorFaker.GenerateCreateDTO();
        Color color = ColorFaker.GenerateColor();
        ColorReadDTO expectedDTO = ColorFaker.GenerateReadDTOFromEntity(color);

        colorRepoMock.Setup(r => r.IsHexAlreadyExistsAsync(createDTO.HexCode, null, default)).ReturnsAsync(false);
        colorRepoMock.Setup(r => r.IsNameAlreadyExistsAsync(createDTO.Name, null, default)).ReturnsAsync(false);
        colorRepoMock.Setup(r => r.IsSlugAlreadyExistsAsync(createDTO.Slug, null, default)).ReturnsAsync(false);
        mapperMock.Setup(m => m.Map<Color>(createDTO)).Returns(color);
        mapperMock.Setup(m => m.Map<ColorReadDTO>(color)).Returns(expectedDTO);

        ColorReadDTO result = await colorService.CreateAsync(createDTO);

        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expectedDTO);
    }

    [Fact]
    public async Task CreateAsync_WhenValidData_SavesAndInvalidatesFilterCache()
    {
        ColorCreateDTO createDTO = ColorFaker.GenerateCreateDTO();
        Color color = ColorFaker.GenerateColor();

        colorRepoMock.Setup(r => r.IsHexAlreadyExistsAsync(createDTO.HexCode, null, default)).ReturnsAsync(false);
        colorRepoMock.Setup(r => r.IsNameAlreadyExistsAsync(createDTO.Name, null, default)).ReturnsAsync(false);
        colorRepoMock.Setup(r => r.IsSlugAlreadyExistsAsync(createDTO.Slug, null, default)).ReturnsAsync(false);
        mapperMock.Setup(m => m.Map<Color>(createDTO)).Returns(color);
        mapperMock.Setup(m => m.Map<ColorReadDTO>(color)).Returns(ColorFaker.GenerateReadDTOFromEntity(color));

        await colorService.CreateAsync(createDTO);

        unitOfWorkMock.Verify(u => u.SaveChangesAsync(default), Times.Once);
        filterCacheInvalidationServiceMock.Verify(c => c.InvalidateAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WhenHexAlreadyExists_ThrowsAlreadyExistsException()
    {
        ColorCreateDTO createDTO = ColorFaker.GenerateCreateDTO();

        colorRepoMock.Setup(r => r.IsHexAlreadyExistsAsync(createDTO.HexCode, null, default)).ReturnsAsync(true);

        Func<Task> act = async () => await colorService.CreateAsync(createDTO);

        await act.Should().ThrowAsync<AlreadyExistsException>().WithMessage($"*{createDTO.HexCode}*");
        unitOfWorkMock.Verify(u => u.SaveChangesAsync(default), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_WhenNameAlreadyExists_ThrowsAlreadyExistsException()
    {
        ColorCreateDTO createDTO = ColorFaker.GenerateCreateDTO();

        colorRepoMock.Setup(r => r.IsHexAlreadyExistsAsync(createDTO.HexCode, null, default)).ReturnsAsync(false);
        colorRepoMock.Setup(r => r.IsNameAlreadyExistsAsync(createDTO.Name, null, default)).ReturnsAsync(true);

        Func<Task> act = async () => await colorService.CreateAsync(createDTO);

        await act.Should().ThrowAsync<AlreadyExistsException>().WithMessage($"*{createDTO.Name}*");
        unitOfWorkMock.Verify(u => u.SaveChangesAsync(default), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_WhenSlugAlreadyExists_ThrowsAlreadyExistsException()
    {
        ColorCreateDTO createDTO = ColorFaker.GenerateCreateDTO();

        colorRepoMock.Setup(r => r.IsHexAlreadyExistsAsync(createDTO.HexCode, null, default)).ReturnsAsync(false);
        colorRepoMock.Setup(r => r.IsNameAlreadyExistsAsync(createDTO.Name, null, default)).ReturnsAsync(false);
        colorRepoMock.Setup(r => r.IsSlugAlreadyExistsAsync(createDTO.Slug, null, default)).ReturnsAsync(true);

        Func<Task> act = async () => await colorService.CreateAsync(createDTO);

        await act.Should().ThrowAsync<AlreadyExistsException>().WithMessage($"*{createDTO.Slug}*");
        unitOfWorkMock.Verify(u => u.SaveChangesAsync(default), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_WhenValidData_ReturnsColorReadDTO()
    {
        Color color = ColorFaker.GenerateColor();
        ColorUpdateDTO updateDTO = ColorFaker.GenerateUpdateDTO();
        ColorReadDTO expectedDTO = ColorFaker.GenerateReadDTO(color.Id);

        colorRepoMock.Setup(r => r.GetByIdAsync(color.Id, default)).ReturnsAsync(color);
        colorRepoMock.Setup(r => r.IsHexAlreadyExistsAsync(updateDTO.HexCode, color.Id, default)).ReturnsAsync(false);
        colorRepoMock.Setup(r => r.IsNameAlreadyExistsAsync(updateDTO.Name, color.Id, default)).ReturnsAsync(false);
        colorRepoMock.Setup(r => r.IsSlugAlreadyExistsAsync(updateDTO.Slug, color.Id, default)).ReturnsAsync(false);
        mapperMock.Setup(m => m.Map(updateDTO, color)).Returns(color);
        mapperMock.Setup(m => m.Map<ColorReadDTO>(color)).Returns(expectedDTO);

        ColorReadDTO result = await colorService.UpdateAsync(color.Id, updateDTO);

        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expectedDTO);
    }

    [Fact]
    public async Task UpdateAsync_WhenValidData_SavesAndInvalidatesAllCaches()
    {
        Color color = ColorFaker.GenerateColor();
        ColorUpdateDTO updateDTO = ColorFaker.GenerateUpdateDTO();

        colorRepoMock.Setup(r => r.GetByIdAsync(color.Id, default)).ReturnsAsync(color);
        colorRepoMock.Setup(r => r.IsHexAlreadyExistsAsync(updateDTO.HexCode, color.Id, default)).ReturnsAsync(false);
        colorRepoMock.Setup(r => r.IsNameAlreadyExistsAsync(updateDTO.Name, color.Id, default)).ReturnsAsync(false);
        colorRepoMock.Setup(r => r.IsSlugAlreadyExistsAsync(updateDTO.Slug, color.Id, default)).ReturnsAsync(false);
        mapperMock.Setup(m => m.Map(updateDTO, color)).Returns(color);
        mapperMock.Setup(m => m.Map<ColorReadDTO>(color)).Returns(ColorFaker.GenerateReadDTO(color.Id));

        await colorService.UpdateAsync(color.Id, updateDTO);

        unitOfWorkMock.Verify(u => u.SaveChangesAsync(default), Times.Once);
        filterCacheInvalidationServiceMock.Verify(c => c.InvalidateAsync(), Times.Once);
        clotheItemInvalidationServiceMock.Verify(c => c.InvalidateAllAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WhenColorNotFound_ThrowsNotFoundException()
    {
        Guid id = Guid.NewGuid();
        ColorUpdateDTO updateDTO = ColorFaker.GenerateUpdateDTO();

        colorRepoMock.Setup(r => r.GetByIdAsync(id, default)).ReturnsAsync((Color?)null);

        Func<Task> act = async () => await colorService.UpdateAsync(id, updateDTO);

        await act.Should().ThrowAsync<NotFoundException>().WithMessage($"*{id}*");
        unitOfWorkMock.Verify(u => u.SaveChangesAsync(default), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_WhenHexAlreadyExists_ThrowsAlreadyExistsException()
    {
        Color color = ColorFaker.GenerateColor();
        ColorUpdateDTO updateDTO = ColorFaker.GenerateUpdateDTO();

        colorRepoMock.Setup(r => r.GetByIdAsync(color.Id, default)).ReturnsAsync(color);
        colorRepoMock.Setup(r => r.IsHexAlreadyExistsAsync(updateDTO.HexCode, color.Id, default)).ReturnsAsync(true);

        Func<Task> act = async () => await colorService.UpdateAsync(color.Id, updateDTO);

        await act.Should().ThrowAsync<AlreadyExistsException>().WithMessage($"*{updateDTO.HexCode}*");
        unitOfWorkMock.Verify(u => u.SaveChangesAsync(default), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_WhenNameAlreadyExists_ThrowsAlreadyExistsException()
    {
        Color color = ColorFaker.GenerateColor();
        ColorUpdateDTO updateDTO = ColorFaker.GenerateUpdateDTO();

        colorRepoMock.Setup(r => r.GetByIdAsync(color.Id, default)).ReturnsAsync(color);
        colorRepoMock.Setup(r => r.IsHexAlreadyExistsAsync(updateDTO.HexCode, color.Id, default)).ReturnsAsync(false);
        colorRepoMock.Setup(r => r.IsNameAlreadyExistsAsync(updateDTO.Name, color.Id, default)).ReturnsAsync(true);

        Func<Task> act = async () => await colorService.UpdateAsync(color.Id, updateDTO);

        await act.Should().ThrowAsync<AlreadyExistsException>().WithMessage($"*{updateDTO.Name}*");
        unitOfWorkMock.Verify(u => u.SaveChangesAsync(default), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_WhenSlugAlreadyExists_ThrowsAlreadyExistsException()
    {
        Color color = ColorFaker.GenerateColor();
        ColorUpdateDTO updateDTO = ColorFaker.GenerateUpdateDTO();

        colorRepoMock.Setup(r => r.GetByIdAsync(color.Id, default)).ReturnsAsync(color);
        colorRepoMock.Setup(r => r.IsHexAlreadyExistsAsync(updateDTO.HexCode, color.Id, default)).ReturnsAsync(false);
        colorRepoMock.Setup(r => r.IsNameAlreadyExistsAsync(updateDTO.Name, color.Id, default)).ReturnsAsync(false);
        colorRepoMock.Setup(r => r.IsSlugAlreadyExistsAsync(updateDTO.Slug, color.Id, default)).ReturnsAsync(true);

        Func<Task> act = async () => await colorService.UpdateAsync(color.Id, updateDTO);

        await act.Should().ThrowAsync<AlreadyExistsException>().WithMessage($"*{updateDTO.Slug}*");
        unitOfWorkMock.Verify(u => u.SaveChangesAsync(default), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_WhenColorExists_DeletesSuccessfully()
    {
        Color color = ColorFaker.GenerateColor();

        colorRepoMock.Setup(r => r.GetByIdAsync(color.Id, default)).ReturnsAsync(color);

        await colorService.DeleteAsync(color.Id);

        colorRepoMock.Verify(r => r.Delete(color), Times.Once);
        unitOfWorkMock.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WhenColorExists_InvalidatesAllCaches()
    {
        Color color = ColorFaker.GenerateColor();

        colorRepoMock.Setup(r => r.GetByIdAsync(color.Id, default)).ReturnsAsync(color);

        await colorService.DeleteAsync(color.Id);

        filterCacheInvalidationServiceMock.Verify(c => c.InvalidateAsync(), Times.Once);
        clotheItemInvalidationServiceMock.Verify(c => c.InvalidateAllAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WhenColorNotFound_ThrowsNotFoundException()
    {
        Guid id = Guid.NewGuid();

        colorRepoMock.Setup(r => r.GetByIdAsync(id, default)).ReturnsAsync((Color?)null);

        Func<Task> act = async () => await colorService.DeleteAsync(id);

        await act.Should().ThrowAsync<NotFoundException>().WithMessage($"*{id}*");
        colorRepoMock.Verify(r => r.Delete(It.IsAny<Color>()), Times.Never);
        unitOfWorkMock.Verify(u => u.SaveChangesAsync(default), Times.Never);
    }
}