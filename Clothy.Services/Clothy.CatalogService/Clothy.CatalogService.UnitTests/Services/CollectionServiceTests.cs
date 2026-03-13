using AutoMapper;
using Clothy.Aggregator.Aggregate.RedisCache;
using Clothy.CatalogService.BLL.DTOs.CollectionDTOs;
using Clothy.CatalogService.BLL.Exceptions;
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

public class CollectionServiceTests
{
    private Mock<IUnitOfWork> unitOfWorkMock;
    private Mock<ICollectionRepository> collectionRepoMock;
    private Mock<IMapper> mapperMock;
    private Mock<IFilterCacheInvalidationService> filterCacheInvalidationServiceMock;
    private Mock<IEntityCacheInvalidationService<ClotheItem>> clotheItemInvalidationServiceMock;

    private CollectionService collectionService;

    public CollectionServiceTests()
    {
        unitOfWorkMock = new Mock<IUnitOfWork>();
        collectionRepoMock = new Mock<ICollectionRepository>();
        mapperMock = new Mock<IMapper>();
        filterCacheInvalidationServiceMock = new Mock<IFilterCacheInvalidationService>();
        clotheItemInvalidationServiceMock = new Mock<IEntityCacheInvalidationService<ClotheItem>>();

        unitOfWorkMock
            .Setup(u => u.Collections)
            .Returns(collectionRepoMock.Object);

        collectionService = new CollectionService(
            unitOfWorkMock.Object,
            mapperMock.Object,
            filterCacheInvalidationServiceMock.Object,
            clotheItemInvalidationServiceMock.Object);
    }
    
    [Fact]
    public async Task GetAllAsync_WhenCollectionsExist_ReturnsListOfCollectionReadDTOs()
    {
        List<Collection> collections = CollectionFaker.GenerateCollections(3);
        List<CollectionReadDTO> dtos = collections.Select(CollectionFaker.GenerateReadDTOFromEntity).ToList();

        collectionRepoMock.Setup(r => r.GetAllAsync(default)).ReturnsAsync(collections);
        mapperMock.Setup(m => m.Map<List<CollectionReadDTO>>(collections)).Returns(dtos);

        List<CollectionReadDTO> result = await collectionService.GetAllAsync();

        result.Should().NotBeNull();
        result.Should().HaveCount(3);
        result.Should().BeEquivalentTo(dtos);
    }

    [Fact]
    public async Task GetAllAsync_WhenNoCollectionsExist_ReturnsEmptyList()
    {
        collectionRepoMock.Setup(r => r.GetAllAsync(default)).ReturnsAsync(new List<Collection>());
        mapperMock.Setup(m => m.Map<List<CollectionReadDTO>>(It.IsAny<List<Collection>>())).Returns(new List<CollectionReadDTO>());

        List<CollectionReadDTO> result = await collectionService.GetAllAsync();

        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }


    [Fact]
    public async Task GetByIdAsync_WhenCollectionExists_ReturnsCollectionReadDTO()
    {
        Collection collection = CollectionFaker.GenerateCollection();
        CollectionReadDTO dto = CollectionFaker.GenerateReadDTOFromEntity(collection);

        collectionRepoMock.Setup(r => r.GetByIdAsync(collection.Id, default)).ReturnsAsync(collection);
        mapperMock.Setup(m => m.Map<CollectionReadDTO>(collection)).Returns(dto);

        CollectionReadDTO result = await collectionService.GetByIdAsync(collection.Id);

        result.Should().NotBeNull();
        result.Id.Should().Be(collection.Id);
        result.Name.Should().Be(collection.Name);
    }

    [Fact]
    public async Task GetByIdAsync_WhenCollectionNotFound_ThrowsNotFoundException()
    {
        Guid id = Guid.NewGuid();

        collectionRepoMock.Setup(r => r.GetByIdAsync(id, default)).ReturnsAsync((Collection?)null);

        Func<Task> act = async () => await collectionService.GetByIdAsync(id);

        await act.Should().ThrowAsync<NotFoundException>().WithMessage($"*{id}*");
    }
    
    [Fact]
    public async Task CreateAsync_WhenValidData_ReturnsCollectionReadDTO()
    {
        CollectionCreateDTO createDTO = CollectionFaker.GenerateCreateDTO();
        Collection collection = CollectionFaker.GenerateCollection();
        CollectionReadDTO expectedDTO = CollectionFaker.GenerateReadDTOFromEntity(collection);

        collectionRepoMock.Setup(r => r.IsNameAlreadyExistsAsync(createDTO.Name, null, default)).ReturnsAsync(false);
        collectionRepoMock.Setup(r => r.IsSlugAlreadyExistsAsync(createDTO.Slug, null, default)).ReturnsAsync(false);
        mapperMock.Setup(m => m.Map<Collection>(createDTO)).Returns(collection);
        mapperMock.Setup(m => m.Map<CollectionReadDTO>(collection)).Returns(expectedDTO);

        CollectionReadDTO result = await collectionService.CreateAsync(createDTO);

        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expectedDTO);
    }

    [Fact]
    public async Task CreateAsync_WhenValidData_SavesAndInvalidatesFilterCache()
    {
        CollectionCreateDTO createDTO = CollectionFaker.GenerateCreateDTO();
        Collection collection = CollectionFaker.GenerateCollection();

        collectionRepoMock.Setup(r => r.IsNameAlreadyExistsAsync(createDTO.Name, null, default)).ReturnsAsync(false);
        collectionRepoMock.Setup(r => r.IsSlugAlreadyExistsAsync(createDTO.Slug, null, default)).ReturnsAsync(false);
        mapperMock.Setup(m => m.Map<Collection>(createDTO)).Returns(collection);
        mapperMock.Setup(m => m.Map<CollectionReadDTO>(collection)).Returns(CollectionFaker.GenerateReadDTOFromEntity(collection));

        await collectionService.CreateAsync(createDTO);

        unitOfWorkMock.Verify(u => u.SaveChangesAsync(default), Times.Once);
        filterCacheInvalidationServiceMock.Verify(c => c.InvalidateAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WhenNameAlreadyExists_ThrowsAlreadyExistsException()
    {
        CollectionCreateDTO createDTO = CollectionFaker.GenerateCreateDTO();

        collectionRepoMock.Setup(r => r.IsNameAlreadyExistsAsync(createDTO.Name, null, default)).ReturnsAsync(true);

        Func<Task> act = async () => await collectionService.CreateAsync(createDTO);

        await act.Should().ThrowAsync<AlreadyExistsException>().WithMessage("*name*");
        unitOfWorkMock.Verify(u => u.SaveChangesAsync(default), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_WhenSlugAlreadyExists_ThrowsAlreadyExistsException()
    {
        CollectionCreateDTO createDTO = CollectionFaker.GenerateCreateDTO();

        collectionRepoMock.Setup(r => r.IsNameAlreadyExistsAsync(createDTO.Name, null, default)).ReturnsAsync(false);
        collectionRepoMock.Setup(r => r.IsSlugAlreadyExistsAsync(createDTO.Slug, null, default)).ReturnsAsync(true);

        Func<Task> act = async () => await collectionService.CreateAsync(createDTO);

        await act.Should().ThrowAsync<AlreadyExistsException>().WithMessage("*slug*");
        unitOfWorkMock.Verify(u => u.SaveChangesAsync(default), Times.Never);
    }
    
    [Fact]
    public async Task UpdateAsync_WhenValidData_ReturnsCollectionReadDTO()
    {
        Collection collection = CollectionFaker.GenerateCollection();
        CollectionUpdateDTO updateDTO = CollectionFaker.GenerateUpdateDTO();
        CollectionReadDTO expectedDTO = CollectionFaker.GenerateReadDTO(collection.Id);

        collectionRepoMock.Setup(r => r.GetByIdAsync(collection.Id, default)).ReturnsAsync(collection);
        collectionRepoMock.Setup(r => r.IsNameAlreadyExistsAsync(updateDTO.Name, collection.Id, default)).ReturnsAsync(false);
        collectionRepoMock.Setup(r => r.IsSlugAlreadyExistsAsync(updateDTO.Slug, collection.Id, default)).ReturnsAsync(false);
        mapperMock.Setup(m => m.Map(updateDTO, collection)).Returns(collection);
        mapperMock.Setup(m => m.Map<CollectionReadDTO>(collection)).Returns(expectedDTO);

        CollectionReadDTO result = await collectionService.UpdateAsync(collection.Id, updateDTO);

        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expectedDTO);
    }

    [Fact]
    public async Task UpdateAsync_WhenValidData_SavesAndInvalidatesAllCaches()
    {
        Collection collection = CollectionFaker.GenerateCollection();
        CollectionUpdateDTO updateDTO = CollectionFaker.GenerateUpdateDTO();

        collectionRepoMock.Setup(r => r.GetByIdAsync(collection.Id, default)).ReturnsAsync(collection);
        collectionRepoMock.Setup(r => r.IsNameAlreadyExistsAsync(updateDTO.Name, collection.Id, default)).ReturnsAsync(false);
        collectionRepoMock.Setup(r => r.IsSlugAlreadyExistsAsync(updateDTO.Slug, collection.Id, default)).ReturnsAsync(false);
        mapperMock.Setup(m => m.Map(updateDTO, collection)).Returns(collection);
        mapperMock.Setup(m => m.Map<CollectionReadDTO>(collection)).Returns(CollectionFaker.GenerateReadDTO(collection.Id));

        await collectionService.UpdateAsync(collection.Id, updateDTO);

        unitOfWorkMock.Verify(u => u.SaveChangesAsync(default), Times.Once);
        filterCacheInvalidationServiceMock.Verify(c => c.InvalidateAsync(), Times.Once);
        clotheItemInvalidationServiceMock.Verify(c => c.InvalidateAllAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WhenCollectionNotFound_ThrowsNotFoundException()
    {
        Guid id = Guid.NewGuid();
        CollectionUpdateDTO updateDTO = CollectionFaker.GenerateUpdateDTO();

        collectionRepoMock.Setup(r => r.GetByIdAsync(id, default)).ReturnsAsync((Collection?)null);

        Func<Task> act = async () => await collectionService.UpdateAsync(id, updateDTO);

        await act.Should().ThrowAsync<NotFoundException>().WithMessage($"*{id}*");
        unitOfWorkMock.Verify(u => u.SaveChangesAsync(default), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_WhenNameAlreadyExists_ThrowsAlreadyExistsException()
    {
        Collection collection = CollectionFaker.GenerateCollection();
        CollectionUpdateDTO updateDTO = CollectionFaker.GenerateUpdateDTO();

        collectionRepoMock.Setup(r => r.GetByIdAsync(collection.Id, default)).ReturnsAsync(collection);
        collectionRepoMock.Setup(r => r.IsNameAlreadyExistsAsync(updateDTO.Name, collection.Id, default)).ReturnsAsync(true);

        Func<Task> act = async () => await collectionService.UpdateAsync(collection.Id, updateDTO);

        await act.Should().ThrowAsync<AlreadyExistsException>().WithMessage("*name*");
        unitOfWorkMock.Verify(u => u.SaveChangesAsync(default), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_WhenSlugAlreadyExists_ThrowsAlreadyExistsException()
    {
        Collection collection = CollectionFaker.GenerateCollection();
        CollectionUpdateDTO updateDTO = CollectionFaker.GenerateUpdateDTO();

        collectionRepoMock.Setup(r => r.GetByIdAsync(collection.Id, default)).ReturnsAsync(collection);
        collectionRepoMock.Setup(r => r.IsNameAlreadyExistsAsync(updateDTO.Name, collection.Id, default)).ReturnsAsync(false);
        collectionRepoMock.Setup(r => r.IsSlugAlreadyExistsAsync(updateDTO.Slug, collection.Id, default)).ReturnsAsync(true);

        Func<Task> act = async () => await collectionService.UpdateAsync(collection.Id, updateDTO);

        await act.Should().ThrowAsync<AlreadyExistsException>().WithMessage("*slug*");
        unitOfWorkMock.Verify(u => u.SaveChangesAsync(default), Times.Never);
    }
    
    [Fact]
    public async Task DeleteAsync_WhenCollectionExists_DeletesSuccessfully()
    {
        Collection collection = CollectionFaker.GenerateCollection();

        collectionRepoMock.Setup(r => r.GetByIdAsync(collection.Id, default)).ReturnsAsync(collection);

        await collectionService.DeleteAsync(collection.Id);

        collectionRepoMock.Verify(r => r.Delete(collection), Times.Once);
        unitOfWorkMock.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WhenCollectionExists_InvalidatesAllCaches()
    {
        Collection collection = CollectionFaker.GenerateCollection();

        collectionRepoMock.Setup(r => r.GetByIdAsync(collection.Id, default)).ReturnsAsync(collection);

        await collectionService.DeleteAsync(collection.Id);

        filterCacheInvalidationServiceMock.Verify(c => c.InvalidateAsync(), Times.Once);
        clotheItemInvalidationServiceMock.Verify(c => c.InvalidateAllAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WhenCollectionNotFound_ThrowsNotFoundException()
    {
        Guid id = Guid.NewGuid();

        collectionRepoMock.Setup(r => r.GetByIdAsync(id, default)).ReturnsAsync((Collection?)null);

        Func<Task> act = async () => await collectionService.DeleteAsync(id);

        await act.Should().ThrowAsync<NotFoundException>().WithMessage($"*{id}*");
        collectionRepoMock.Verify(r => r.Delete(It.IsAny<Collection>()), Times.Never);
        unitOfWorkMock.Verify(u => u.SaveChangesAsync(default), Times.Never);
    }
}