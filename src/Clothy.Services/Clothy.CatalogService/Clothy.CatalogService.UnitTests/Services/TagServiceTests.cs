using AutoMapper;
using Clothy.Aggregator.Aggregate.RedisCache;
using Clothy.CatalogService.BLL.DTOs.TagDTOs;
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

public class TagServiceTests
{
    private Mock<IUnitOfWork> unitOfWorkMock;
    private Mock<ITagRepository> tagRepoMock;
    private Mock<IMapper> mapperMock;
    private Mock<IFilterCacheInvalidationService> filterCacheInvalidationServiceMock;

    private TagService tagService;

    public TagServiceTests()
    {
        unitOfWorkMock = new Mock<IUnitOfWork>();
        tagRepoMock = new Mock<ITagRepository>();
        mapperMock = new Mock<IMapper>();
        filterCacheInvalidationServiceMock = new Mock<IFilterCacheInvalidationService>();

        unitOfWorkMock
            .Setup(u => u.Tags)
            .Returns(tagRepoMock.Object);

        tagService = new TagService(
            unitOfWorkMock.Object,
            mapperMock.Object,
            filterCacheInvalidationServiceMock.Object);
    }

    [Fact]
    public async Task GetAllAsync_WhenTagsExist_ReturnsListOfTagReadDTOs()
    {
        List<Tag> tags = TagFaker.GenerateTags(3);
        List<TagReadDTO> dtos = tags.Select(TagFaker.GenerateReadDTOFromEntity).ToList();

        tagRepoMock.Setup(r => r.GetAllAsync(default)).ReturnsAsync(tags);
        mapperMock.Setup(m => m.Map<List<TagReadDTO>>(tags)).Returns(dtos);

        List<TagReadDTO> result = await tagService.GetAllAsync();

        result.Should().NotBeNull();
        result.Should().HaveCount(3);
        result.Should().BeEquivalentTo(dtos);
    }

    [Fact]
    public async Task GetAllAsync_WhenNoTagsExist_ReturnsEmptyList()
    {
        tagRepoMock.Setup(r => r.GetAllAsync(default)).ReturnsAsync(new List<Tag>());
        mapperMock.Setup(m => m.Map<List<TagReadDTO>>(It.IsAny<List<Tag>>())).Returns(new List<TagReadDTO>());

        List<TagReadDTO> result = await tagService.GetAllAsync();

        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetByIdAsync_WhenTagExists_ReturnsTagReadDTO()
    {
        Tag tag = TagFaker.GenerateTag();
        TagReadDTO dto = TagFaker.GenerateReadDTOFromEntity(tag);

        tagRepoMock.Setup(r => r.GetByIdAsync(tag.Id, default)).ReturnsAsync(tag);
        mapperMock.Setup(m => m.Map<TagReadDTO>(tag)).Returns(dto);

        TagReadDTO result = await tagService.GetByIdAsync(tag.Id);

        result.Should().NotBeNull();
        result.Id.Should().Be(tag.Id);
        result.Name.Should().Be(tag.Name);
    }

    [Fact]
    public async Task GetByIdAsync_WhenTagNotFound_ThrowsNotFoundException()
    {
        Guid id = Guid.NewGuid();

        tagRepoMock.Setup(r => r.GetByIdAsync(id, default)).ReturnsAsync((Tag?)null);

        Func<Task> act = async () => await tagService.GetByIdAsync(id);

        await act.Should().ThrowAsync<NotFoundException>().WithMessage($"*{id}*");
    }

    [Fact]
    public async Task CreateAsync_WhenValidData_ReturnsTagReadDTO()
    {
        TagCreateDTO createDTO = TagFaker.GenerateCreateDTO();
        Tag tag = TagFaker.GenerateTag();
        TagReadDTO expectedDTO = TagFaker.GenerateReadDTOFromEntity(tag);

        tagRepoMock.Setup(r => r.IsNameAlreadyExistsAsync(createDTO.Name, null, default)).ReturnsAsync(false);
        tagRepoMock.Setup(r => r.IsSlugAlreadyExistsAsync(createDTO.Slug, null, default)).ReturnsAsync(false);
        mapperMock.Setup(m => m.Map<Tag>(createDTO)).Returns(tag);
        mapperMock.Setup(m => m.Map<TagReadDTO>(tag)).Returns(expectedDTO);

        TagReadDTO result = await tagService.CreateAsync(createDTO);

        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expectedDTO);
    }

    [Fact]
    public async Task CreateAsync_WhenValidData_SavesAndInvalidatesFilterCache()
    {
        TagCreateDTO createDTO = TagFaker.GenerateCreateDTO();
        Tag tag = TagFaker.GenerateTag();

        tagRepoMock.Setup(r => r.IsNameAlreadyExistsAsync(createDTO.Name, null, default)).ReturnsAsync(false);
        tagRepoMock.Setup(r => r.IsSlugAlreadyExistsAsync(createDTO.Slug, null, default)).ReturnsAsync(false);
        mapperMock.Setup(m => m.Map<Tag>(createDTO)).Returns(tag);
        mapperMock.Setup(m => m.Map<TagReadDTO>(tag)).Returns(TagFaker.GenerateReadDTOFromEntity(tag));

        await tagService.CreateAsync(createDTO);

        unitOfWorkMock.Verify(u => u.SaveChangesAsync(default), Times.Once);
        filterCacheInvalidationServiceMock.Verify(c => c.InvalidateAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WhenNameAlreadyExists_ThrowsAlreadyExistsException()
    {
        TagCreateDTO createDTO = TagFaker.GenerateCreateDTO();

        tagRepoMock.Setup(r => r.IsNameAlreadyExistsAsync(createDTO.Name, null, default)).ReturnsAsync(true);

        Func<Task> act = async () => await tagService.CreateAsync(createDTO);

        await act.Should().ThrowAsync<AlreadyExistsException>().WithMessage("*name*");
        unitOfWorkMock.Verify(u => u.SaveChangesAsync(default), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_WhenSlugAlreadyExists_ThrowsAlreadyExistsException()
    {
        TagCreateDTO createDTO = TagFaker.GenerateCreateDTO();

        tagRepoMock.Setup(r => r.IsNameAlreadyExistsAsync(createDTO.Name, null, default)).ReturnsAsync(false);
        tagRepoMock.Setup(r => r.IsSlugAlreadyExistsAsync(createDTO.Slug, null, default)).ReturnsAsync(true);

        Func<Task> act = async () => await tagService.CreateAsync(createDTO);

        await act.Should().ThrowAsync<AlreadyExistsException>().WithMessage("*slug*");
        unitOfWorkMock.Verify(u => u.SaveChangesAsync(default), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_WhenValidData_ReturnsTagReadDTO()
    {
        Tag tag = TagFaker.GenerateTag();
        TagUpdateDTO updateDTO = TagFaker.GenerateUpdateDTO();
        TagReadDTO expectedDTO = TagFaker.GenerateReadDTO(tag.Id);

        tagRepoMock.Setup(r => r.GetByIdAsync(tag.Id, default)).ReturnsAsync(tag);
        tagRepoMock.Setup(r => r.IsNameAlreadyExistsAsync(updateDTO.Name, tag.Id, default)).ReturnsAsync(false);
        tagRepoMock.Setup(r => r.IsSlugAlreadyExistsAsync(updateDTO.Slug, tag.Id, default)).ReturnsAsync(false);
        mapperMock.Setup(m => m.Map(updateDTO, tag)).Returns(tag);
        mapperMock.Setup(m => m.Map<TagReadDTO>(tag)).Returns(expectedDTO);

        TagReadDTO result = await tagService.UpdateAsync(tag.Id, updateDTO);

        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expectedDTO);
    }

    [Fact]
    public async Task UpdateAsync_WhenValidData_SavesAndInvalidatesFilterCache()
    {
        Tag tag = TagFaker.GenerateTag();
        TagUpdateDTO updateDTO = TagFaker.GenerateUpdateDTO();

        tagRepoMock.Setup(r => r.GetByIdAsync(tag.Id, default)).ReturnsAsync(tag);
        tagRepoMock.Setup(r => r.IsNameAlreadyExistsAsync(updateDTO.Name, tag.Id, default)).ReturnsAsync(false);
        tagRepoMock.Setup(r => r.IsSlugAlreadyExistsAsync(updateDTO.Slug, tag.Id, default)).ReturnsAsync(false);
        mapperMock.Setup(m => m.Map(updateDTO, tag)).Returns(tag);
        mapperMock.Setup(m => m.Map<TagReadDTO>(tag)).Returns(TagFaker.GenerateReadDTO(tag.Id));

        await tagService.UpdateAsync(tag.Id, updateDTO);

        unitOfWorkMock.Verify(u => u.SaveChangesAsync(default), Times.Once);
        filterCacheInvalidationServiceMock.Verify(c => c.InvalidateAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WhenTagNotFound_ThrowsNotFoundException()
    {
        Guid id = Guid.NewGuid();
        TagUpdateDTO updateDTO = TagFaker.GenerateUpdateDTO();

        tagRepoMock.Setup(r => r.GetByIdAsync(id, default)).ReturnsAsync((Tag?)null);

        Func<Task> act = async () => await tagService.UpdateAsync(id, updateDTO);

        await act.Should().ThrowAsync<NotFoundException>().WithMessage($"*{id}*");
        unitOfWorkMock.Verify(u => u.SaveChangesAsync(default), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_WhenNameAlreadyExists_ThrowsAlreadyExistsException()
    {
        Tag tag = TagFaker.GenerateTag();
        TagUpdateDTO updateDTO = TagFaker.GenerateUpdateDTO();

        tagRepoMock.Setup(r => r.GetByIdAsync(tag.Id, default)).ReturnsAsync(tag);
        tagRepoMock.Setup(r => r.IsNameAlreadyExistsAsync(updateDTO.Name, tag.Id, default)).ReturnsAsync(true);

        Func<Task> act = async () => await tagService.UpdateAsync(tag.Id, updateDTO);

        await act.Should().ThrowAsync<AlreadyExistsException>().WithMessage("*name*");
        unitOfWorkMock.Verify(u => u.SaveChangesAsync(default), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_WhenSlugAlreadyExists_ThrowsAlreadyExistsException()
    {
        Tag tag = TagFaker.GenerateTag();
        TagUpdateDTO updateDTO = TagFaker.GenerateUpdateDTO();

        tagRepoMock.Setup(r => r.GetByIdAsync(tag.Id, default)).ReturnsAsync(tag);
        tagRepoMock.Setup(r => r.IsNameAlreadyExistsAsync(updateDTO.Name, tag.Id, default)).ReturnsAsync(false);
        tagRepoMock.Setup(r => r.IsSlugAlreadyExistsAsync(updateDTO.Slug, tag.Id, default)).ReturnsAsync(true);

        Func<Task> act = async () => await tagService.UpdateAsync(tag.Id, updateDTO);

        await act.Should().ThrowAsync<AlreadyExistsException>().WithMessage("*slug*");
        unitOfWorkMock.Verify(u => u.SaveChangesAsync(default), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_WhenTagExists_DeletesSuccessfully()
    {
        Tag tag = TagFaker.GenerateTag();

        tagRepoMock.Setup(r => r.GetByIdAsync(tag.Id, default)).ReturnsAsync(tag);

        await tagService.DeleteAsync(tag.Id);

        tagRepoMock.Verify(r => r.Delete(tag), Times.Once);
        unitOfWorkMock.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WhenTagExists_InvalidatesFilterCache()
    {
        Tag tag = TagFaker.GenerateTag();

        tagRepoMock.Setup(r => r.GetByIdAsync(tag.Id, default)).ReturnsAsync(tag);

        await tagService.DeleteAsync(tag.Id);

        filterCacheInvalidationServiceMock.Verify(c => c.InvalidateAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WhenTagNotFound_ThrowsNotFoundException()
    {
        Guid id = Guid.NewGuid();

        tagRepoMock.Setup(r => r.GetByIdAsync(id, default)).ReturnsAsync((Tag?)null);

        Func<Task> act = async () => await tagService.DeleteAsync(id);

        await act.Should().ThrowAsync<NotFoundException>().WithMessage($"*{id}*");
        tagRepoMock.Verify(r => r.Delete(It.IsAny<Tag>()), Times.Never);
        unitOfWorkMock.Verify(u => u.SaveChangesAsync(default), Times.Never);
    }
}