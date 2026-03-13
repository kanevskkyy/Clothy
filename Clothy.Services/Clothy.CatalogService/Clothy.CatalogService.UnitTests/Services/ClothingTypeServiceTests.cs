using AutoMapper;
using Clothy.Aggregator.Aggregate.RedisCache;
using Clothy.CatalogService.BLL.DTOs.ClothingTypeDTOs;
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

public class ClothingTypeServiceTests
{
    private Mock<IUnitOfWork> unitOfWorkMock;
    private Mock<IClothingTypeRepository> clotheTypeRepoMock;
    private Mock<IMapper> mapperMock;
    private Mock<IFilterCacheInvalidationService> filterCacheInvalidationServiceMock;
    private Mock<IEntityCacheInvalidationService<ClotheItem>> clotheItemInvalidationServiceMock;

    private ClothingTypeService clothingTypeService;

    public ClothingTypeServiceTests()
    {
        unitOfWorkMock = new Mock<IUnitOfWork>();
        clotheTypeRepoMock = new Mock<IClothingTypeRepository>();
        mapperMock = new Mock<IMapper>();
        filterCacheInvalidationServiceMock = new Mock<IFilterCacheInvalidationService>();
        clotheItemInvalidationServiceMock = new Mock<IEntityCacheInvalidationService<ClotheItem>>();

        unitOfWorkMock
            .Setup(u => u.ClothingTypes)
            .Returns(clotheTypeRepoMock.Object);

        clothingTypeService = new ClothingTypeService(unitOfWorkMock.Object,
            mapperMock.Object,
            filterCacheInvalidationServiceMock.Object,
            clotheItemInvalidationServiceMock.Object);
    }

    [Fact]
    public async Task GetAllAsync_WhenClothingTypesExist_ReturnsListOfClothingTypeReadDTOs()
    {
        List<ClothingType> clothingTypes = ClothingTypeFaker.GenerateClothingTypes(3);
        List<ClothingTypeReadDTO> clothingTypeReadDtos = clothingTypes.Select(ClothingTypeFaker.GenerateReadDTOFromEntity).ToList();

        clotheTypeRepoMock
            .Setup(p => p.GetAllAsync(default))
            .ReturnsAsync(clothingTypes);

        mapperMock
            .Setup(m => m.Map<List<ClothingTypeReadDTO>>(clothingTypes))
            .Returns(clothingTypeReadDtos);

        List<ClothingTypeReadDTO> result = await clothingTypeService.GetAllAsync();

        result.Should().NotBeNull();
        result.Should().HaveCount(3);
        result.Should().BeEquivalentTo(clothingTypeReadDtos);
    }

    [Fact]
    public async Task GetAllAsync_WhenNoClothingTypesExist_ReturnsEmptyList()
    {
        clotheTypeRepoMock.Setup(p => p.GetAllAsync(default))
            .ReturnsAsync(new List<ClothingType>());

        mapperMock.Setup(m => m.Map<List<ClothingTypeReadDTO>>(It.IsAny<List<ClothingType>>()))
            .Returns(new List<ClothingTypeReadDTO>());

        List<ClothingTypeReadDTO>? result = await clothingTypeService.GetAllAsync();

        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetByIdAsync_WhenClothingTypeExists_ReturnsClothingTypeReadDTO()
    {
        ClothingType clothingType = ClothingTypeFaker.GenerateClothingType();
        ClothingTypeReadDTO clothingTypeReadDto = ClothingTypeFaker.GenerateReadDTOFromEntity(clothingType);

        clotheTypeRepoMock.Setup(p => p.GetByIdAsync(clothingType.Id, default))
            .ReturnsAsync(clothingType);

        mapperMock.Setup(m => m.Map<ClothingTypeReadDTO>(clothingType))
            .Returns(clothingTypeReadDto);

        ClothingTypeReadDTO? result = await clothingTypeService.GetByIdAsync(clothingType.Id);

        result.Should().NotBeNull();
        result!.Id.Should().Be(clothingType.Id);
        result.Name.Should().Be(clothingType.Name);
    }

    [Fact]
    public async Task GetByIdAsync_WhenClothingTypeNotFound_ThrowsNotFoundException()
    {
        Guid clothingTypeId = Guid.NewGuid();

        clotheTypeRepoMock
            .Setup(p => p.GetByIdAsync(clothingTypeId, default))
            .ReturnsAsync((ClothingType?)null);

        Func<Task> act = async () => await clothingTypeService.GetByIdAsync(clothingTypeId);

        await act.Should().ThrowAsync<NotFoundException>().WithMessage($"*{clothingTypeId}*");
    }
    
    [Fact]
    public async Task CreateAsync_WhenValidData_ReturnsClothingTypeReadDTO()
    {
        ClothingTypeCreateDTO createDTO = ClothingTypeFaker.GenerateCreateDTO();
        ClothingType clothingType = ClothingTypeFaker.GenerateClothingType();
        ClothingTypeReadDTO expectedDTO = ClothingTypeFaker.GenerateReadDTOFromEntity(clothingType);

        clotheTypeRepoMock.Setup(r => r.IsNameAlreadyExistsAsync(createDTO.Name, null, default)).ReturnsAsync(false);
        clotheTypeRepoMock.Setup(r => r.IsSlugAlreadyExistsAsync(createDTO.Slug, null, default)).ReturnsAsync(false);
        mapperMock.Setup(m => m.Map<ClothingType>(createDTO)).Returns(clothingType);
        mapperMock.Setup(m => m.Map<ClothingTypeReadDTO>(clothingType)).Returns(expectedDTO);

        ClothingTypeReadDTO result = await clothingTypeService.CreateAsync(createDTO);

        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expectedDTO);
    }

    [Fact]
    public async Task CreateAsync_WhenValidData_SavesAndInvalidatesFilterCache()
    {
        ClothingTypeCreateDTO createDTO = ClothingTypeFaker.GenerateCreateDTO();
        ClothingType clothingType = ClothingTypeFaker.GenerateClothingType();

        clotheTypeRepoMock.Setup(r => r.IsNameAlreadyExistsAsync(createDTO.Name, null, default)).ReturnsAsync(false);
        clotheTypeRepoMock.Setup(r => r.IsSlugAlreadyExistsAsync(createDTO.Slug, null, default)).ReturnsAsync(false);
        mapperMock.Setup(m => m.Map<ClothingType>(createDTO)).Returns(clothingType);
        mapperMock.Setup(m => m.Map<ClothingTypeReadDTO>(clothingType)).Returns(ClothingTypeFaker.GenerateReadDTOFromEntity(clothingType));

        await clothingTypeService.CreateAsync(createDTO);

        unitOfWorkMock.Verify(u => u.SaveChangesAsync(default), Times.Once);
        filterCacheInvalidationServiceMock.Verify(c => c.InvalidateAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WhenNameAlreadyExists_ThrowsAlreadyExistsException()
    {
        ClothingTypeCreateDTO createDTO = ClothingTypeFaker.GenerateCreateDTO();

        clotheTypeRepoMock
            .Setup(r => r.IsNameAlreadyExistsAsync(createDTO.Name, null, default))
            .ReturnsAsync(true);

        Func<Task> act = async () => await clothingTypeService.CreateAsync(createDTO);

        await act.Should().ThrowAsync<AlreadyExistsException>().WithMessage("*name*");
        unitOfWorkMock.Verify(u => u.SaveChangesAsync(default), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_WhenSlugAlreadyExists_ThrowsAlreadyExistsException()
    {
        ClothingTypeCreateDTO createDTO = ClothingTypeFaker.GenerateCreateDTO();

        clotheTypeRepoMock.Setup(r => r.IsNameAlreadyExistsAsync(createDTO.Name, null, default)).ReturnsAsync(false);
        clotheTypeRepoMock
            .Setup(r => r.IsSlugAlreadyExistsAsync(createDTO.Slug, null, default))
            .ReturnsAsync(true);

        Func<Task> act = async () => await clothingTypeService.CreateAsync(createDTO);

        await act.Should().ThrowAsync<AlreadyExistsException>().WithMessage("*slug*");
        unitOfWorkMock.Verify(u => u.SaveChangesAsync(default), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_WhenValidData_ReturnsClothingTypeReadDTO()
    {
        ClothingType clothingType = ClothingTypeFaker.GenerateClothingType();
        ClothingTypeUpdateDTO updateDTO = ClothingTypeFaker.GenerateUpdateDTO();
        ClothingTypeReadDTO expectedDTO = ClothingTypeFaker.GenerateReadDTO(clothingType.Id);

        clotheTypeRepoMock.Setup(r => r.GetByIdAsync(clothingType.Id, default)).ReturnsAsync(clothingType);
        clotheTypeRepoMock.Setup(r => r.IsNameAlreadyExistsAsync(updateDTO.Name, clothingType.Id, default)).ReturnsAsync(false);
        clotheTypeRepoMock.Setup(r => r.IsSlugAlreadyExistsAsync(updateDTO.Slug, clothingType.Id, default)).ReturnsAsync(false);
        mapperMock.Setup(m => m.Map(updateDTO, clothingType)).Returns(clothingType);
        mapperMock.Setup(m => m.Map<ClothingTypeReadDTO>(clothingType)).Returns(expectedDTO);

        ClothingTypeReadDTO result = await clothingTypeService.UpdateAsync(clothingType.Id, updateDTO);

        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expectedDTO);
    }

    [Fact]
    public async Task UpdateAsync_WhenValidData_SavesAndInvalidatesAllCaches()
    {
        ClothingType clothingType = ClothingTypeFaker.GenerateClothingType();
        ClothingTypeUpdateDTO updateDTO = ClothingTypeFaker.GenerateUpdateDTO();

        clotheTypeRepoMock.Setup(r => r.GetByIdAsync(clothingType.Id, default)).ReturnsAsync(clothingType);
        clotheTypeRepoMock.Setup(r => r.IsNameAlreadyExistsAsync(updateDTO.Name, clothingType.Id, default)).ReturnsAsync(false);
        clotheTypeRepoMock.Setup(r => r.IsSlugAlreadyExistsAsync(updateDTO.Slug, clothingType.Id, default)).ReturnsAsync(false);
        mapperMock.Setup(m => m.Map(updateDTO, clothingType)).Returns(clothingType);
        mapperMock.Setup(m => m.Map<ClothingTypeReadDTO>(clothingType)).Returns(ClothingTypeFaker.GenerateReadDTO(clothingType.Id));

        await clothingTypeService.UpdateAsync(clothingType.Id, updateDTO);

        unitOfWorkMock.Verify(u => u.SaveChangesAsync(default), Times.Once);
        filterCacheInvalidationServiceMock.Verify(c => c.InvalidateAsync(), Times.Once);
        clotheItemInvalidationServiceMock.Verify(c => c.InvalidateAllAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WhenClothingTypeNotFound_ThrowsNotFoundException()
    {
        Guid id = Guid.NewGuid();
        ClothingTypeUpdateDTO updateDTO = ClothingTypeFaker.GenerateUpdateDTO();

        clotheTypeRepoMock
            .Setup(r => r.GetByIdAsync(id, default))
            .ReturnsAsync((ClothingType?)null);

        Func<Task> act = async () => await clothingTypeService.UpdateAsync(id, updateDTO);

        await act.Should().ThrowAsync<NotFoundException>().WithMessage($"*{id}*");
        unitOfWorkMock.Verify(u => u.SaveChangesAsync(default), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_WhenNameAlreadyExists_ThrowsAlreadyExistsException()
    {
        ClothingType clothingType = ClothingTypeFaker.GenerateClothingType();
        ClothingTypeUpdateDTO updateDTO = ClothingTypeFaker.GenerateUpdateDTO();

        clotheTypeRepoMock.Setup(r => r.GetByIdAsync(clothingType.Id, default)).ReturnsAsync(clothingType);
        clotheTypeRepoMock
            .Setup(r => r.IsNameAlreadyExistsAsync(updateDTO.Name, clothingType.Id, default))
            .ReturnsAsync(true);

        Func<Task> act = async () => await clothingTypeService.UpdateAsync(clothingType.Id, updateDTO);

        await act.Should().ThrowAsync<AlreadyExistsException>().WithMessage("*name*");
        unitOfWorkMock.Verify(u => u.SaveChangesAsync(default), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_WhenSlugAlreadyExists_ThrowsAlreadyExistsException()
    {
        ClothingType clothingType = ClothingTypeFaker.GenerateClothingType();
        ClothingTypeUpdateDTO updateDTO = ClothingTypeFaker.GenerateUpdateDTO();

        clotheTypeRepoMock.Setup(r => r.GetByIdAsync(clothingType.Id, default)).ReturnsAsync(clothingType);
        clotheTypeRepoMock.Setup(r => r.IsNameAlreadyExistsAsync(updateDTO.Name, clothingType.Id, default)).ReturnsAsync(false);
        clotheTypeRepoMock
            .Setup(r => r.IsSlugAlreadyExistsAsync(updateDTO.Slug, clothingType.Id, default))
            .ReturnsAsync(true);

        Func<Task> act = async () => await clothingTypeService.UpdateAsync(clothingType.Id, updateDTO);

        await act.Should().ThrowAsync<AlreadyExistsException>().WithMessage("*slug*");
        unitOfWorkMock.Verify(u => u.SaveChangesAsync(default), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_WhenClothingTypeExists_DeletesSuccessfully()
    {
        ClothingType clothingType = ClothingTypeFaker.GenerateClothingType();

        clotheTypeRepoMock
            .Setup(r => r.GetByIdAsync(clothingType.Id, default))
            .ReturnsAsync(clothingType);

        await clothingTypeService.DeleteAsync(clothingType.Id);

        clotheTypeRepoMock.Verify(r => r.Delete(clothingType), Times.Once);
        unitOfWorkMock.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WhenClothingTypeExists_InvalidatesAllCaches()
    {
        ClothingType clothingType = ClothingTypeFaker.GenerateClothingType();

        clotheTypeRepoMock
            .Setup(r => r.GetByIdAsync(clothingType.Id, default))
            .ReturnsAsync(clothingType);

        await clothingTypeService.DeleteAsync(clothingType.Id);

        filterCacheInvalidationServiceMock.Verify(c => c.InvalidateAsync(), Times.Once);
        clotheItemInvalidationServiceMock.Verify(c => c.InvalidateAllAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WhenClothingTypeNotFound_ThrowsNotFoundException()
    {
        Guid id = Guid.NewGuid();

        clotheTypeRepoMock
            .Setup(r => r.GetByIdAsync(id, default))
            .ReturnsAsync((ClothingType?)null);

        Func<Task> act = async () => await clothingTypeService.DeleteAsync(id);

        await act.Should().ThrowAsync<NotFoundException>().WithMessage($"*{id}*");
        clotheTypeRepoMock.Verify(r => r.Delete(It.IsAny<ClothingType>()), Times.Never);
        unitOfWorkMock.Verify(u => u.SaveChangesAsync(default), Times.Never);
    }
}