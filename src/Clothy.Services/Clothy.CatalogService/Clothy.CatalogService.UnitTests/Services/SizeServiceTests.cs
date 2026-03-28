using AutoMapper;
using Clothy.Aggregator.Aggregate.RedisCache;
using Clothy.CatalogService.BLL.DTOs.SizeDTOs;
using Clothy.CatalogService.BLL.Exceptions;
using Clothy.CatalogService.BLL.Services;
using Clothy.CatalogService.DAL.Interfaces;
using Clothy.CatalogService.DAL.UOW;
using Clothy.CatalogService.Domain.Entities.Catalog;
using Clothy.CatalogService.UnitTests.Helpers;
using Clothy.Shared.Helpers.Exceptions;
using FluentAssertions;
using Moq;
using Xunit;

namespace Clothy.CatalogService.UnitTests.Services;

public class SizeServiceTests
{
    private Mock<IUnitOfWork> unitOfWorkMock;
    private Mock<ISizeRepository> sizeRepoMock;
    private Mock<IMapper> mapperMock;
    private Mock<IFilterCacheInvalidationService> filterCacheInvalidationServiceMock;

    private SizeService sizeService;

    public SizeServiceTests()
    {
        unitOfWorkMock = new Mock<IUnitOfWork>();
        sizeRepoMock = new Mock<ISizeRepository>();
        mapperMock = new Mock<IMapper>();
        filterCacheInvalidationServiceMock = new Mock<IFilterCacheInvalidationService>();

        unitOfWorkMock
            .Setup(u => u.Sizes)
            .Returns(sizeRepoMock.Object);

        sizeService = new SizeService(
            unitOfWorkMock.Object,
            mapperMock.Object,
            filterCacheInvalidationServiceMock.Object);
    }
    
    [Fact]
    public async Task GetAllAsync_WhenSizesExist_ReturnsListOfSizeReadDTOs()
    {
        List<Size> sizes = SizeFaker.GenerateSizes(3);
        List<SizeReadDTO> dtos = sizes.Select(SizeFaker.GenerateReadDTOFromEntity).ToList();

        sizeRepoMock.Setup(r => r.GetAllAsync(default)).ReturnsAsync(sizes);
        mapperMock.Setup(m => m.Map<List<SizeReadDTO>>(sizes)).Returns(dtos);

        List<SizeReadDTO> result = await sizeService.GetAllAsync();

        result.Should().NotBeNull();
        result.Should().HaveCount(3);
        result.Should().BeEquivalentTo(dtos);
    }

    [Fact]
    public async Task GetAllAsync_WhenNoSizesExist_ReturnsEmptyList()
    {
        sizeRepoMock.Setup(r => r.GetAllAsync(default)).ReturnsAsync(new List<Size>());
        mapperMock.Setup(m => m.Map<List<SizeReadDTO>>(It.IsAny<List<Size>>())).Returns(new List<SizeReadDTO>());

        List<SizeReadDTO> result = await sizeService.GetAllAsync();

        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }
    
    [Fact]
    public async Task GetByIdAsync_WhenSizeExists_ReturnsSizeReadDTO()
    {
        Size size = SizeFaker.GenerateSize();
        SizeReadDTO dto = SizeFaker.GenerateReadDTOFromEntity(size);

        sizeRepoMock.Setup(r => r.GetByIdAsync(size.Id, default)).ReturnsAsync(size);
        mapperMock.Setup(m => m.Map<SizeReadDTO>(size)).Returns(dto);

        SizeReadDTO result = await sizeService.GetByIdAsync(size.Id);

        result.Should().NotBeNull();
        result.Id.Should().Be(size.Id);
        result.Name.Should().Be(size.Name);
    }

    [Fact]
    public async Task GetByIdAsync_WhenSizeNotFound_ThrowsNotFoundException()
    {
        Guid id = Guid.NewGuid();

        sizeRepoMock.Setup(r => r.GetByIdAsync(id, default)).ReturnsAsync((Size?)null);

        Func<Task> act = async () => await sizeService.GetByIdAsync(id);

        await act.Should().ThrowAsync<NotFoundException>().WithMessage($"*{id}*");
    }
    
    [Fact]
    public async Task CreateAsync_WhenValidData_ReturnsSizeReadDTO()
    {
        SizeCreateDTO createDTO = SizeFaker.GenerateCreateDTO();
        Size size = SizeFaker.GenerateSize();
        SizeReadDTO expectedDTO = SizeFaker.GenerateReadDTOFromEntity(size);

        sizeRepoMock.Setup(r => r.IsNameAlreadyExistsAsync(createDTO.Name, null, default)).ReturnsAsync(false);
        mapperMock.Setup(m => m.Map<Size>(createDTO)).Returns(size);
        mapperMock.Setup(m => m.Map<SizeReadDTO>(size)).Returns(expectedDTO);

        SizeReadDTO result = await sizeService.CreateAsync(createDTO);

        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expectedDTO);
    }

    [Fact]
    public async Task CreateAsync_WhenValidData_SavesAndInvalidatesFilterCache()
    {
        SizeCreateDTO createDTO = SizeFaker.GenerateCreateDTO();
        Size size = SizeFaker.GenerateSize();

        sizeRepoMock.Setup(r => r.IsNameAlreadyExistsAsync(createDTO.Name, null, default)).ReturnsAsync(false);
        mapperMock.Setup(m => m.Map<Size>(createDTO)).Returns(size);
        mapperMock.Setup(m => m.Map<SizeReadDTO>(size)).Returns(SizeFaker.GenerateReadDTOFromEntity(size));

        await sizeService.CreateAsync(createDTO);

        unitOfWorkMock.Verify(u => u.SaveChangesAsync(default), Times.Once);
        filterCacheInvalidationServiceMock.Verify(c => c.InvalidateAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WhenNameAlreadyExists_ThrowsAlreadyExistsException()
    {
        SizeCreateDTO createDTO = SizeFaker.GenerateCreateDTO();

        sizeRepoMock.Setup(r => r.IsNameAlreadyExistsAsync(createDTO.Name, null, default)).ReturnsAsync(true);

        Func<Task> act = async () => await sizeService.CreateAsync(createDTO);

        await act.Should().ThrowAsync<AlreadyExistsException>().WithMessage("*name*");
        unitOfWorkMock.Verify(u => u.SaveChangesAsync(default), Times.Never);
    }
    
    [Fact]
    public async Task UpdateAsync_WhenValidData_ReturnsSizeReadDTO()
    {
        Size size = SizeFaker.GenerateSize();
        SizeUpdateDTO updateDTO = SizeFaker.GenerateUpdateDTO();
        SizeReadDTO expectedDTO = SizeFaker.GenerateReadDTO(size.Id);

        sizeRepoMock.Setup(r => r.GetByIdAsync(size.Id, default)).ReturnsAsync(size);
        sizeRepoMock.Setup(r => r.IsNameAlreadyExistsAsync(updateDTO.Name, size.Id, default)).ReturnsAsync(false);
        mapperMock.Setup(m => m.Map(updateDTO, size)).Returns(size);
        mapperMock.Setup(m => m.Map<SizeReadDTO>(size)).Returns(expectedDTO);

        SizeReadDTO result = await sizeService.UpdateAsync(size.Id, updateDTO);

        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expectedDTO);
    }

    [Fact]
    public async Task UpdateAsync_WhenValidData_SavesAndInvalidatesFilterCache()
    {
        Size size = SizeFaker.GenerateSize();
        SizeUpdateDTO updateDTO = SizeFaker.GenerateUpdateDTO();

        sizeRepoMock.Setup(r => r.GetByIdAsync(size.Id, default)).ReturnsAsync(size);
        sizeRepoMock.Setup(r => r.IsNameAlreadyExistsAsync(updateDTO.Name, size.Id, default)).ReturnsAsync(false);
        mapperMock.Setup(m => m.Map(updateDTO, size)).Returns(size);
        mapperMock.Setup(m => m.Map<SizeReadDTO>(size)).Returns(SizeFaker.GenerateReadDTO(size.Id));

        await sizeService.UpdateAsync(size.Id, updateDTO);

        unitOfWorkMock.Verify(u => u.SaveChangesAsync(default), Times.Once);
        filterCacheInvalidationServiceMock.Verify(c => c.InvalidateAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WhenSizeNotFound_ThrowsNotFoundException()
    {
        Guid id = Guid.NewGuid();
        SizeUpdateDTO updateDTO = SizeFaker.GenerateUpdateDTO();

        sizeRepoMock.Setup(r => r.GetByIdAsync(id, default)).ReturnsAsync((Size?)null);

        Func<Task> act = async () => await sizeService.UpdateAsync(id, updateDTO);

        await act.Should().ThrowAsync<NotFoundException>().WithMessage($"*{id}*");
        unitOfWorkMock.Verify(u => u.SaveChangesAsync(default), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_WhenNameAlreadyExists_ThrowsAlreadyExistsException()
    {
        Size size = SizeFaker.GenerateSize();
        SizeUpdateDTO updateDTO = SizeFaker.GenerateUpdateDTO();

        sizeRepoMock.Setup(r => r.GetByIdAsync(size.Id, default)).ReturnsAsync(size);
        sizeRepoMock.Setup(r => r.IsNameAlreadyExistsAsync(updateDTO.Name, size.Id, default)).ReturnsAsync(true);

        Func<Task> act = async () => await sizeService.UpdateAsync(size.Id, updateDTO);

        await act.Should().ThrowAsync<AlreadyExistsException>().WithMessage("*name*");
        unitOfWorkMock.Verify(u => u.SaveChangesAsync(default), Times.Never);
    }
    
    [Fact]
    public async Task DeleteAsync_WhenSizeExists_DeletesSuccessfully()
    {
        Size size = SizeFaker.GenerateSize();

        sizeRepoMock.Setup(r => r.GetByIdAsync(size.Id, default)).ReturnsAsync(size);

        await sizeService.DeleteAsync(size.Id);

        sizeRepoMock.Verify(r => r.Delete(size), Times.Once);
        unitOfWorkMock.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WhenSizeExists_InvalidatesFilterCache()
    {
        Size size = SizeFaker.GenerateSize();

        sizeRepoMock.Setup(r => r.GetByIdAsync(size.Id, default)).ReturnsAsync(size);

        await sizeService.DeleteAsync(size.Id);

        filterCacheInvalidationServiceMock.Verify(c => c.InvalidateAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WhenSizeNotFound_ThrowsNotFoundException()
    {
        Guid id = Guid.NewGuid();

        sizeRepoMock.Setup(r => r.GetByIdAsync(id, default)).ReturnsAsync((Size?)null);

        Func<Task> act = async () => await sizeService.DeleteAsync(id);

        await act.Should().ThrowAsync<NotFoundException>().WithMessage($"*{id}*");
        sizeRepoMock.Verify(r => r.Delete(It.IsAny<Size>()), Times.Never);
        unitOfWorkMock.Verify(u => u.SaveChangesAsync(default), Times.Never);
    }
}