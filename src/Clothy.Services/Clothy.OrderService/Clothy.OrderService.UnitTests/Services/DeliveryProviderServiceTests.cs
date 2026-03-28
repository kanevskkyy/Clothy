using AutoMapper;
using Clothy.OrderService.BLL.DTOs.DeliveryProviderDTOs;
using Clothy.OrderService.BLL.Services;
using Clothy.OrderService.DAL.Interfaces;
using Clothy.OrderService.DAL.UOW;
using Clothy.OrderService.Domain.Entities;
using Clothy.Shared.Cache.Interfaces;
using Clothy.Shared.Helpers.CloudinaryConfig.ImageService;
using Clothy.Shared.Helpers.Exceptions;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;

namespace Clothy.OrderService.UnitTests.Services;

public class DeliveryProviderServiceTests
{
    private Mock<IUnitOfWork> unitOfWorkMock;
    private Mock<IDeliveryProviderRepository> deliveryProviderRepoMock;
    private Mock<IMapper> mapperMock;
    private Mock<IImageService> imageServiceMock;
    private Mock<IEntityCacheService> cacheServiceMock;
    private Mock<IEntityCacheInvalidationService<DeliveryProvider>> cacheInvalidationMock;
    private Mock<IEntityCacheInvalidationService<PickupPoints>> pickupPointInvalidationMock;

    private DeliveryProviderService deliveryProviderService;

    public DeliveryProviderServiceTests()
    {
        unitOfWorkMock = new Mock<IUnitOfWork>();
        deliveryProviderRepoMock = new Mock<IDeliveryProviderRepository>();
        mapperMock = new Mock<IMapper>();
        imageServiceMock = new Mock<IImageService>();
        cacheServiceMock = new Mock<IEntityCacheService>();
        cacheInvalidationMock = new Mock<IEntityCacheInvalidationService<DeliveryProvider>>();
        pickupPointInvalidationMock = new Mock<IEntityCacheInvalidationService<PickupPoints>>();

        unitOfWorkMock.Setup(u => u.DeliveryProviders).Returns(deliveryProviderRepoMock.Object);

        deliveryProviderService = new DeliveryProviderService(
            unitOfWorkMock.Object,
            mapperMock.Object,
            imageServiceMock.Object,
            cacheServiceMock.Object,
            cacheInvalidationMock.Object,
            pickupPointInvalidationMock.Object);
    }

    private void SetupCacheMiss<T>()
    {
        cacheServiceMock
            .Setup(c => c.GetOrSetAsync(
                It.IsAny<string>(),
                It.IsAny<Func<Task<T>>>(),
                It.IsAny<TimeSpan>(),
                It.IsAny<TimeSpan>()))
            .Returns<string, Func<Task<T>>, TimeSpan, TimeSpan>(
                (key, factory, m, r) => factory());
    }
    
    [Fact]
    public async Task GetAllAsync_WhenProvidersExist_ReturnsListOfDeliveryProviderReadDTOs()
    {
        List<DeliveryProvider> providers = new List<DeliveryProvider>
        {
            new DeliveryProvider { Id = Guid.NewGuid(), Name = "Nova Poshta" },
            new DeliveryProvider { Id = Guid.NewGuid(), Name = "Ukrposhta" }
        };
        List<DeliveryProviderReadDTO> dtos = providers.Select(p => new DeliveryProviderReadDTO { Id = p.Id, Name = p.Name }).ToList();

        SetupCacheMiss<List<DeliveryProviderReadDTO>?>();
        deliveryProviderRepoMock.Setup(r => r.GetAllAsync(default, null)).ReturnsAsync(providers);
        mapperMock.Setup(m => m.Map<List<DeliveryProviderReadDTO>>(providers)).Returns(dtos);

        List<DeliveryProviderReadDTO> result = await deliveryProviderService.GetAllAsync();

        result.Should().NotBeNull();
        result.Should().HaveCount(2);
    }
    

    [Fact]
    public async Task GetByIdAsync_WhenProviderExists_ReturnsDeliveryProviderReadDTO()
    {
        DeliveryProvider provider = new DeliveryProvider { Id = Guid.NewGuid(), Name = "Nova Poshta" };
        DeliveryProviderReadDTO dto = new DeliveryProviderReadDTO { Id = provider.Id, Name = provider.Name };

        SetupCacheMiss<DeliveryProviderReadDTO?>();
        deliveryProviderRepoMock.Setup(r => r.GetByIdAsync(provider.Id, default, null)).ReturnsAsync(provider);
        mapperMock.Setup(m => m.Map<DeliveryProviderReadDTO>(provider)).Returns(dto);

        DeliveryProviderReadDTO result = await deliveryProviderService.GetByIdAsync(provider.Id);

        result.Should().NotBeNull();
        result.Id.Should().Be(provider.Id);
    }

    [Fact]
    public async Task GetByIdAsync_WhenProviderNotFound_ThrowsNotFoundException()
    {
        Guid id = Guid.NewGuid();

        SetupCacheMiss<DeliveryProviderReadDTO?>();
        deliveryProviderRepoMock.Setup(r => r.GetByIdAsync(id, default, null)).ReturnsAsync((DeliveryProvider?)null);

        Func<Task> act = async () => await deliveryProviderService.GetByIdAsync(id);

        await act.Should().ThrowAsync<NotFoundException>().WithMessage($"*{id}*");
    }
    
    [Fact]
    public async Task CreateAsync_WhenNameAlreadyExists_ThrowsAlreadyExistsException()
    {
        DeliveryProviderCreateDTO createDTO = new DeliveryProviderCreateDTO
        {
            Name = "Nova Poshta",
            Icon = new Mock<IFormFile>().Object
        };

        deliveryProviderRepoMock
            .Setup(r => r.ExistsByNameAsync(createDTO.Name, null, default))
            .ReturnsAsync(true);

        Func<Task> act = async () => await deliveryProviderService.CreateAsync(createDTO);

        await act.Should().ThrowAsync<AlreadyExistsException>().WithMessage($"*{createDTO.Name}*");
        imageServiceMock.Verify(s => s.UploadAsync(It.IsAny<IFormFile>(), It.IsAny<string>(), It.IsAny<bool>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_WhenValidData_UploadsIconAndReturnsDTO()
    {
        DeliveryProviderCreateDTO createDTO = new DeliveryProviderCreateDTO
        {
            Name = "Nova Poshta",
            Icon = new Mock<IFormFile>().Object
        };
        DeliveryProvider provider = new DeliveryProvider { Id = Guid.NewGuid(), Name = createDTO.Name };
        DeliveryProviderReadDTO expectedDTO = new DeliveryProviderReadDTO { Id = provider.Id, Name = provider.Name };

        deliveryProviderRepoMock.Setup(r => r.ExistsByNameAsync(createDTO.Name, null, default)).ReturnsAsync(false);
        imageServiceMock.Setup(s => s.UploadAsync(createDTO.Icon, "delivery-providers", true)).ReturnsAsync("https://icon.jpg");
        mapperMock.Setup(m => m.Map<DeliveryProvider>(createDTO)).Returns(provider);
        deliveryProviderRepoMock.Setup(r => r.AddAsync(provider, default, null)).ReturnsAsync(provider.Id);
        mapperMock.Setup(m => m.Map<DeliveryProviderReadDTO>(provider)).Returns(expectedDTO);

        DeliveryProviderReadDTO result = await deliveryProviderService.CreateAsync(createDTO);

        result.Should().NotBeNull();
        imageServiceMock.Verify(s => s.UploadAsync(createDTO.Icon, "delivery-providers", true), Times.Once);
        unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Once);
        cacheInvalidationMock.Verify(c => c.InvalidateAllAsync(), Times.Once);
    }


    [Fact]
    public async Task UpdateAsync_WhenProviderNotFound_ThrowsNotFoundException()
    {
        Guid id = Guid.NewGuid();
        DeliveryProviderUpdateDTO updateDTO = new DeliveryProviderUpdateDTO { Name = "Updated" };

        deliveryProviderRepoMock.Setup(r => r.GetByIdAsync(id, default, null)).ReturnsAsync((DeliveryProvider?)null);

        Func<Task> act = async () => await deliveryProviderService.UpdateAsync(id, updateDTO);

        await act.Should().ThrowAsync<NotFoundException>().WithMessage($"*{id}*");
    }

    [Fact]
    public async Task UpdateAsync_WhenNameAlreadyExists_ThrowsAlreadyExistsException()
    {
        DeliveryProvider provider = new DeliveryProvider { Id = Guid.NewGuid(), Name = "Old Name" };
        DeliveryProviderUpdateDTO updateDTO = new DeliveryProviderUpdateDTO { Name = "Existing Name" };

        deliveryProviderRepoMock.Setup(r => r.GetByIdAsync(provider.Id, default, null)).ReturnsAsync(provider);
        deliveryProviderRepoMock.Setup(r => r.ExistsByNameAsync(updateDTO.Name, provider.Id, default)).ReturnsAsync(true);

        Func<Task> act = async () => await deliveryProviderService.UpdateAsync(provider.Id, updateDTO);

        await act.Should().ThrowAsync<AlreadyExistsException>().WithMessage($"*{updateDTO.Name}*");
        unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_WhenNewIconProvided_DeletesOldAndUploadsNew()
    {
        DeliveryProvider provider = new DeliveryProvider
        {
            Id = Guid.NewGuid(),
            Name = "Nova Poshta",
            IconUrl = "https://old-icon.jpg"
        };
        Mock<IFormFile> newIconMock = new Mock<IFormFile>();
        DeliveryProviderUpdateDTO updateDTO = new DeliveryProviderUpdateDTO
        {
            Name = "Nova Poshta Updated",
            Icon = newIconMock.Object
        };
        DeliveryProviderReadDTO expectedDTO = new DeliveryProviderReadDTO { Id = provider.Id };

        deliveryProviderRepoMock.Setup(r => r.GetByIdAsync(provider.Id, default, null)).ReturnsAsync(provider);
        deliveryProviderRepoMock.Setup(r => r.ExistsByNameAsync(updateDTO.Name, provider.Id, default)).ReturnsAsync(false);
        imageServiceMock.Setup(s => s.UploadAsync(newIconMock.Object, "delivery-providers", default)).ReturnsAsync("https://new-icon.jpg");
        mapperMock.Setup(m => m.Map(updateDTO, provider)).Returns(provider);
        mapperMock.Setup(m => m.Map<DeliveryProviderReadDTO>(provider)).Returns(expectedDTO);

        await deliveryProviderService.UpdateAsync(provider.Id, updateDTO);

        imageServiceMock.Verify(s => s.DeleteImageAsync("https://old-icon.jpg"), Times.Once);
        imageServiceMock.Verify(s => s.UploadAsync(newIconMock.Object, "delivery-providers", default), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WhenNoNewIcon_DoesNotTouchImageService()
    {
        DeliveryProvider provider = new DeliveryProvider { Id = Guid.NewGuid(), Name = "Nova Poshta", IconUrl = "https://icon.jpg" };
        DeliveryProviderUpdateDTO updateDTO = new DeliveryProviderUpdateDTO { Name = "Updated", Icon = null };
        DeliveryProviderReadDTO expectedDTO = new DeliveryProviderReadDTO { Id = provider.Id };

        deliveryProviderRepoMock.Setup(r => r.GetByIdAsync(provider.Id, default, null)).ReturnsAsync(provider);
        deliveryProviderRepoMock.Setup(r => r.ExistsByNameAsync(updateDTO.Name, provider.Id, default)).ReturnsAsync(false);
        mapperMock.Setup(m => m.Map(updateDTO, provider)).Returns(provider);
        mapperMock.Setup(m => m.Map<DeliveryProviderReadDTO>(provider)).Returns(expectedDTO);

        await deliveryProviderService.UpdateAsync(provider.Id, updateDTO);

        imageServiceMock.Verify(s => s.DeleteImageAsync(It.IsAny<string>()), Times.Never);
        imageServiceMock.Verify(s => s.UploadAsync(It.IsAny<IFormFile>(), It.IsAny<string>(), default), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_WhenSaveSucceeds_InvalidatesBothCaches()
    {
        DeliveryProvider provider = new DeliveryProvider { Id = Guid.NewGuid(), Name = "Nova Poshta" };
        DeliveryProviderUpdateDTO updateDTO = new DeliveryProviderUpdateDTO { Name = "Updated", Icon = null };
        DeliveryProviderReadDTO expectedDTO = new DeliveryProviderReadDTO { Id = provider.Id };

        deliveryProviderRepoMock.Setup(r => r.GetByIdAsync(provider.Id, default, null)).ReturnsAsync(provider);
        deliveryProviderRepoMock.Setup(r => r.ExistsByNameAsync(updateDTO.Name, provider.Id, default)).ReturnsAsync(false);
        mapperMock.Setup(m => m.Map(updateDTO, provider)).Returns(provider);
        mapperMock.Setup(m => m.Map<DeliveryProviderReadDTO>(provider)).Returns(expectedDTO);

        await deliveryProviderService.UpdateAsync(provider.Id, updateDTO);

        unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Once);
        cacheInvalidationMock.Verify(c => c.InvalidateByIdAsync(provider.Id), Times.Once);
        cacheInvalidationMock.Verify(c => c.InvalidateAllAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WhenProviderNotFound_ThrowsNotFoundException()
    {
        Guid id = Guid.NewGuid();

        deliveryProviderRepoMock.Setup(r => r.GetByIdAsync(id, default, null)).ReturnsAsync((DeliveryProvider?)null);

        Func<Task> act = async () => await deliveryProviderService.DeleteAsync(id);

        await act.Should().ThrowAsync<NotFoundException>().WithMessage($"*{id}*");
        unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_WhenProviderHasIcon_DeletesIcon()
    {
        DeliveryProvider provider = new DeliveryProvider { Id = Guid.NewGuid(), IconUrl = "https://icon.jpg" };

        deliveryProviderRepoMock.Setup(r => r.GetByIdAsync(provider.Id, default, null)).ReturnsAsync(provider);

        await deliveryProviderService.DeleteAsync(provider.Id);

        imageServiceMock.Verify(s => s.DeleteImageAsync("https://icon.jpg"), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WhenProviderExists_DeletesAndInvalidatesAllCaches()
    {
        DeliveryProvider provider = new DeliveryProvider { Id = Guid.NewGuid(), IconUrl = null };

        deliveryProviderRepoMock.Setup(r => r.GetByIdAsync(provider.Id, default, null)).ReturnsAsync(provider);

        await deliveryProviderService.DeleteAsync(provider.Id);

        deliveryProviderRepoMock.Verify(r => r.DeleteAsync(provider.Id, default, null), Times.Once);
        unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Once);
        cacheInvalidationMock.Verify(c => c.InvalidateByIdAsync(provider.Id), Times.Once);
        cacheInvalidationMock.Verify(c => c.InvalidateAllAsync(), Times.Once);
        pickupPointInvalidationMock.Verify(c => c.InvalidateAllAsync(), Times.Once);
    }
}