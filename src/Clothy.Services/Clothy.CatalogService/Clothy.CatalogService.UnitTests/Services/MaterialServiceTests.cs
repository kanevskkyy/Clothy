using AutoMapper;
using Clothy.Aggregator.Aggregate.RedisCache;
using Clothy.CatalogService.BLL.DTOs.MaterialDTOs;
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

public class MaterialServiceTests
{
    private Mock<IUnitOfWork> unitOfWorkMock;
    private Mock<IMaterialRepository> materialRepoMock;
    private Mock<IMapper> mapperMock;
    private Mock<IFilterCacheInvalidationService> filterCacheInvalidationServiceMock;

    private MaterialService materialService;

    public MaterialServiceTests()
    {
        unitOfWorkMock = new Mock<IUnitOfWork>();
        materialRepoMock = new Mock<IMaterialRepository>();
        mapperMock = new Mock<IMapper>();
        filterCacheInvalidationServiceMock = new Mock<IFilterCacheInvalidationService>();

        unitOfWorkMock
            .Setup(u => u.Materials)
            .Returns(materialRepoMock.Object);

        materialService = new MaterialService(
            unitOfWorkMock.Object,
            mapperMock.Object,
            filterCacheInvalidationServiceMock.Object);
    }

    [Fact]
    public async Task GetAllAsync_WhenMaterialsExist_ReturnsListOfMaterialReadDTOs()
    {
        List<Material> materials = MaterialFaker.GenerateMaterials(3);
        List<MaterialReadDTO> dtos = materials.Select(MaterialFaker.GenerateReadDTOFromEntity).ToList();

        materialRepoMock.Setup(r => r.GetAllAsync(default)).ReturnsAsync(materials);
        mapperMock.Setup(m => m.Map<List<MaterialReadDTO>>(materials)).Returns(dtos);

        List<MaterialReadDTO> result = await materialService.GetAllAsync();

        result.Should().NotBeNull();
        result.Should().HaveCount(3);
        result.Should().BeEquivalentTo(dtos);
    }

    [Fact]
    public async Task GetAllAsync_WhenNoMaterialsExist_ReturnsEmptyList()
    {
        materialRepoMock.Setup(r => r.GetAllAsync(default)).ReturnsAsync(new List<Material>());
        mapperMock.Setup(m => m.Map<List<MaterialReadDTO>>(It.IsAny<List<Material>>())).Returns(new List<MaterialReadDTO>());

        List<MaterialReadDTO> result = await materialService.GetAllAsync();

        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetByIdAsync_WhenMaterialExists_ReturnsMaterialReadDTO()
    {
        Material material = MaterialFaker.GenerateMaterial();
        MaterialReadDTO dto = MaterialFaker.GenerateReadDTOFromEntity(material);

        materialRepoMock.Setup(r => r.GetByIdAsync(material.Id, default)).ReturnsAsync(material);
        mapperMock.Setup(m => m.Map<MaterialReadDTO>(material)).Returns(dto);

        MaterialReadDTO result = await materialService.GetByIdAsync(material.Id);

        result.Should().NotBeNull();
        result.Id.Should().Be(material.Id);
        result.Name.Should().Be(material.Name);
    }

    [Fact]
    public async Task GetByIdAsync_WhenMaterialNotFound_ThrowsNotFoundException()
    {
        Guid id = Guid.NewGuid();

        materialRepoMock.Setup(r => r.GetByIdAsync(id, default)).ReturnsAsync((Material?)null);

        Func<Task> act = async () => await materialService.GetByIdAsync(id);

        await act.Should().ThrowAsync<NotFoundException>().WithMessage($"*{id}*");
    }

    [Fact]
    public async Task CreateAsync_WhenValidData_ReturnsMaterialReadDTO()
    {
        MaterialCreateDTO createDTO = MaterialFaker.GenerateCreateDTO();
        Material material = MaterialFaker.GenerateMaterial();
        MaterialReadDTO expectedDTO = MaterialFaker.GenerateReadDTOFromEntity(material);

        materialRepoMock.Setup(r => r.IsNameAlreadyExistsAsync(createDTO.Name, null, default)).ReturnsAsync(false);
        materialRepoMock.Setup(r => r.IsSlugAlreadyExistsAsync(createDTO.Slug, null, default)).ReturnsAsync(false);
        mapperMock.Setup(m => m.Map<Material>(createDTO)).Returns(material);
        mapperMock.Setup(m => m.Map<MaterialReadDTO>(material)).Returns(expectedDTO);

        MaterialReadDTO result = await materialService.CreateAsync(createDTO);

        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expectedDTO);
    }

    [Fact]
    public async Task CreateAsync_WhenValidData_SavesAndInvalidatesFilterCache()
    {
        MaterialCreateDTO createDTO = MaterialFaker.GenerateCreateDTO();
        Material material = MaterialFaker.GenerateMaterial();

        materialRepoMock.Setup(r => r.IsNameAlreadyExistsAsync(createDTO.Name, null, default)).ReturnsAsync(false);
        materialRepoMock.Setup(r => r.IsSlugAlreadyExistsAsync(createDTO.Slug, null, default)).ReturnsAsync(false);
        mapperMock.Setup(m => m.Map<Material>(createDTO)).Returns(material);
        mapperMock.Setup(m => m.Map<MaterialReadDTO>(material)).Returns(MaterialFaker.GenerateReadDTOFromEntity(material));

        await materialService.CreateAsync(createDTO);

        unitOfWorkMock.Verify(u => u.SaveChangesAsync(default), Times.Once);
        filterCacheInvalidationServiceMock.Verify(c => c.InvalidateAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WhenNameAlreadyExists_ThrowsAlreadyExistsException()
    {
        MaterialCreateDTO createDTO = MaterialFaker.GenerateCreateDTO();

        materialRepoMock.Setup(r => r.IsNameAlreadyExistsAsync(createDTO.Name, null, default)).ReturnsAsync(true);

        Func<Task> act = async () => await materialService.CreateAsync(createDTO);

        await act.Should().ThrowAsync<AlreadyExistsException>().WithMessage("*name*");
        unitOfWorkMock.Verify(u => u.SaveChangesAsync(default), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_WhenSlugAlreadyExists_ThrowsAlreadyExistsException()
    {
        MaterialCreateDTO createDTO = MaterialFaker.GenerateCreateDTO();

        materialRepoMock.Setup(r => r.IsNameAlreadyExistsAsync(createDTO.Name, null, default)).ReturnsAsync(false);
        materialRepoMock.Setup(r => r.IsSlugAlreadyExistsAsync(createDTO.Slug, null, default)).ReturnsAsync(true);

        Func<Task> act = async () => await materialService.CreateAsync(createDTO);

        await act.Should().ThrowAsync<AlreadyExistsException>().WithMessage("*slug*");
        unitOfWorkMock.Verify(u => u.SaveChangesAsync(default), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_WhenValidData_ReturnsMaterialReadDTO()
    {
        Material material = MaterialFaker.GenerateMaterial();
        MaterialUpdateDTO updateDTO = MaterialFaker.GenerateUpdateDTO();
        MaterialReadDTO expectedDTO = MaterialFaker.GenerateReadDTO(material.Id);

        materialRepoMock.Setup(r => r.GetByIdAsync(material.Id, default)).ReturnsAsync(material);
        materialRepoMock.Setup(r => r.IsNameAlreadyExistsAsync(updateDTO.Name, material.Id, default)).ReturnsAsync(false);
        materialRepoMock.Setup(r => r.IsSlugAlreadyExistsAsync(updateDTO.Slug, material.Id, default)).ReturnsAsync(false);
        mapperMock.Setup(m => m.Map(updateDTO, material)).Returns(material);
        mapperMock.Setup(m => m.Map<MaterialReadDTO>(material)).Returns(expectedDTO);

        MaterialReadDTO result = await materialService.UpdateAsync(material.Id, updateDTO);

        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expectedDTO);
    }

    [Fact]
    public async Task UpdateAsync_WhenValidData_SavesAndInvalidatesFilterCache()
    {
        Material material = MaterialFaker.GenerateMaterial();
        MaterialUpdateDTO updateDTO = MaterialFaker.GenerateUpdateDTO();

        materialRepoMock.Setup(r => r.GetByIdAsync(material.Id, default)).ReturnsAsync(material);
        materialRepoMock.Setup(r => r.IsNameAlreadyExistsAsync(updateDTO.Name, material.Id, default)).ReturnsAsync(false);
        materialRepoMock.Setup(r => r.IsSlugAlreadyExistsAsync(updateDTO.Slug, material.Id, default)).ReturnsAsync(false);
        mapperMock.Setup(m => m.Map(updateDTO, material)).Returns(material);
        mapperMock.Setup(m => m.Map<MaterialReadDTO>(material)).Returns(MaterialFaker.GenerateReadDTO(material.Id));

        await materialService.UpdateAsync(material.Id, updateDTO);

        unitOfWorkMock.Verify(u => u.SaveChangesAsync(default), Times.Once);
        filterCacheInvalidationServiceMock.Verify(c => c.InvalidateAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WhenMaterialNotFound_ThrowsNotFoundException()
    {
        Guid id = Guid.NewGuid();
        MaterialUpdateDTO updateDTO = MaterialFaker.GenerateUpdateDTO();

        materialRepoMock.Setup(r => r.GetByIdAsync(id, default)).ReturnsAsync((Material?)null);

        Func<Task> act = async () => await materialService.UpdateAsync(id, updateDTO);

        await act.Should().ThrowAsync<NotFoundException>().WithMessage($"*{id}*");
        unitOfWorkMock.Verify(u => u.SaveChangesAsync(default), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_WhenNameAlreadyExists_ThrowsAlreadyExistsException()
    {
        Material material = MaterialFaker.GenerateMaterial();
        MaterialUpdateDTO updateDTO = MaterialFaker.GenerateUpdateDTO();

        materialRepoMock.Setup(r => r.GetByIdAsync(material.Id, default)).ReturnsAsync(material);
        materialRepoMock.Setup(r => r.IsNameAlreadyExistsAsync(updateDTO.Name, material.Id, default)).ReturnsAsync(true);

        Func<Task> act = async () => await materialService.UpdateAsync(material.Id, updateDTO);

        await act.Should().ThrowAsync<AlreadyExistsException>().WithMessage("*name*");
        unitOfWorkMock.Verify(u => u.SaveChangesAsync(default), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_WhenSlugAlreadyExists_ThrowsAlreadyExistsException()
    {
        Material material = MaterialFaker.GenerateMaterial();
        MaterialUpdateDTO updateDTO = MaterialFaker.GenerateUpdateDTO();

        materialRepoMock.Setup(r => r.GetByIdAsync(material.Id, default)).ReturnsAsync(material);
        materialRepoMock.Setup(r => r.IsNameAlreadyExistsAsync(updateDTO.Name, material.Id, default)).ReturnsAsync(false);
        materialRepoMock.Setup(r => r.IsSlugAlreadyExistsAsync(updateDTO.Slug, material.Id, default)).ReturnsAsync(true);

        Func<Task> act = async () => await materialService.UpdateAsync(material.Id, updateDTO);

        await act.Should().ThrowAsync<AlreadyExistsException>().WithMessage("*slug*");
        unitOfWorkMock.Verify(u => u.SaveChangesAsync(default), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_WhenMaterialExists_DeletesSuccessfully()
    {
        Material material = MaterialFaker.GenerateMaterial();

        materialRepoMock.Setup(r => r.GetByIdAsync(material.Id, default)).ReturnsAsync(material);

        await materialService.DeleteAsync(material.Id);

        materialRepoMock.Verify(r => r.Delete(material), Times.Once);
        unitOfWorkMock.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WhenMaterialExists_InvalidatesFilterCache()
    {
        Material material = MaterialFaker.GenerateMaterial();

        materialRepoMock.Setup(r => r.GetByIdAsync(material.Id, default)).ReturnsAsync(material);

        await materialService.DeleteAsync(material.Id);

        filterCacheInvalidationServiceMock.Verify(c => c.InvalidateAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WhenMaterialNotFound_ThrowsNotFoundException()
    {
        Guid id = Guid.NewGuid();

        materialRepoMock.Setup(r => r.GetByIdAsync(id, default)).ReturnsAsync((Material?)null);

        Func<Task> act = async () => await materialService.DeleteAsync(id);

        await act.Should().ThrowAsync<NotFoundException>().WithMessage($"*{id}*");
        materialRepoMock.Verify(r => r.Delete(It.IsAny<Material>()), Times.Never);
        unitOfWorkMock.Verify(u => u.SaveChangesAsync(default), Times.Never);
    }
}