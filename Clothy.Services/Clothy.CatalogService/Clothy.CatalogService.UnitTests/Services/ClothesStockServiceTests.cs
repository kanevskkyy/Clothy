using System.Diagnostics.Metrics;
using AutoMapper;
using Clothy.Aggregator.Aggregate.RedisCache;
using Clothy.CatalogService.BLL.DTOs.ClotheStocksDTOs;
using Clothy.CatalogService.BLL.Interfaces;
using Clothy.CatalogService.BLL.Services;
using Clothy.CatalogService.DAL.Interfaces;
using Clothy.CatalogService.DAL.UOW;
using Clothy.CatalogService.Domain.Entities.Stock;
using Clothy.CatalogService.UnitTests.Helpers;
using Clothy.Shared.Cache.Interfaces;
using Clothy.Shared.Helpers.Exceptions;
using FluentAssertions;
using Moq;
using Xunit;

namespace Clothy.CatalogService.UnitTests.Services;

public class ClothesStockServiceTests
    {
        private Mock<IUnitOfWork> unitOfWorkMock;
        private Mock<IClothesStockRepository> stockRepoMock;
        private Mock<IMapper> mapperMock;
        private Mock<IEntityCacheService> cacheServiceMock;
        private Mock<IEntityCacheInvalidationService<ClothesStock>> cacheInvalidationMock;
        private Mock<IFilterCacheInvalidationService> filterCacheInvalidationMock;
        private Mock<IStockNotificationService> stockNotificationServiceMock;
 
        private ClothesStockService clothesStockService;
 
        public ClothesStockServiceTests()
        {
            unitOfWorkMock = new Mock<IUnitOfWork>();
            stockRepoMock = new Mock<IClothesStockRepository>();
            mapperMock = new Mock<IMapper>();
            cacheServiceMock = new Mock<IEntityCacheService>();
            cacheInvalidationMock = new Mock<IEntityCacheInvalidationService<ClothesStock>>();
            filterCacheInvalidationMock = new Mock<IFilterCacheInvalidationService>();
            stockNotificationServiceMock = new Mock<IStockNotificationService>();
 
            unitOfWorkMock.Setup(u => u.ClothesStocks).Returns(stockRepoMock.Object);
 
            Meter meter = new Meter("test.stock.meter");
 
            clothesStockService = new ClothesStockService(
                unitOfWorkMock.Object,
                mapperMock.Object,
                cacheServiceMock.Object,
                cacheInvalidationMock.Object,
                meter,
                filterCacheInvalidationMock.Object,
                stockNotificationServiceMock.Object
            );
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
                    (key, factory, memTtl, redisTtl) => factory());
        }
        
        [Fact]
        public async Task GetByIdWithDetailsAsync_WhenStockExists_ReturnsClothesStockReadDTO()
        {
            ClothesStock stock = ClothesStockFaker.GenerateStock();
            ClothesStockReadDTO expectedDTO = ClothesStockFaker.GenerateReadDTO(stock.Id);
 
            SetupCacheMiss<ClothesStockReadDTO?>();
 
            stockRepoMock
                .Setup(r => r.GetByIdWithDetailsAsync(stock.Id, default))
                .ReturnsAsync(stock);
 
            mapperMock
                .Setup(m => m.Map<ClothesStockReadDTO>(stock))
                .Returns(expectedDTO);
 
            ClothesStockReadDTO result = await clothesStockService.GetByIdWithDetailsAsync(stock.Id);
 
            result.Should().NotBeNull();
            result.Id.Should().Be(expectedDTO.Id);
        }
 
        [Fact]
        public async Task GetByIdWithDetailsAsync_WhenStockNotFound_ThrowsNotFoundException()
        {
            Guid id = Guid.NewGuid();
 
            SetupCacheMiss<ClothesStockReadDTO?>();
 
            stockRepoMock
                .Setup(r => r.GetByIdWithDetailsAsync(id, default))
                .ReturnsAsync((ClothesStock?)null);
 
            Func<Task> act = async () => await clothesStockService.GetByIdWithDetailsAsync(id);
 
            await act.Should().ThrowAsync<NotFoundException>()
                .WithMessage($"*{id}*");
        }
        
        [Fact]
        public async Task UpdateAsync_WhenStockExists_ReturnsMappedDTO()
        {
            ClothesStock stock = ClothesStockFaker.GenerateStock(quantity: 5);
            ClothesStockUpdateDTO updateDTO = ClothesStockFaker.GenerateUpdateDTO(quantity: 20);
            ClothesStockReadDTO expectedDTO = ClothesStockFaker.GenerateReadDTO(stock.Id);
 
            stockRepoMock
                .Setup(r => r.GetByIdWithDetailsAsync(stock.Id, default))
                .ReturnsAsync(stock);
 
            mapperMock
                .Setup(m => m.Map(updateDTO, stock))
                .Callback(() => stock.Quantity = updateDTO.Quantity)
                .Returns(stock);
 
            mapperMock
                .Setup(m => m.Map<ClothesStockReadDTO>(stock))
                .Returns(expectedDTO);
 
            ClothesStockReadDTO result = await clothesStockService.UpdateAsync(stock.Id, updateDTO);
 
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(expectedDTO);
        }
 
        [Fact]
        public async Task UpdateAsync_WhenStockNotFound_ThrowsNotFoundException()
        {
            Guid id = Guid.NewGuid();
            ClothesStockUpdateDTO updateDTO = ClothesStockFaker.GenerateUpdateDTO();
 
            stockRepoMock
                .Setup(r => r.GetByIdWithDetailsAsync(id, default))
                .ReturnsAsync((ClothesStock?)null);
 
            Func<Task> act = async () => await clothesStockService.UpdateAsync(id, updateDTO);
 
            await act.Should().ThrowAsync<NotFoundException>()
                .WithMessage($"*{id}*");
 
            unitOfWorkMock.Verify(u => u.SaveChangesAsync(default), Times.Never);
        }
 
        [Fact]
        public async Task UpdateAsync_WhenStockWasZeroAndNowPositive_NotifiesSubscribers()
        {
            ClothesStock stock = ClothesStockFaker.GenerateStock(quantity: 0);
            ClothesStockUpdateDTO updateDTO = ClothesStockFaker.GenerateUpdateDTO(quantity: 5);
 
            stockRepoMock
                .Setup(r => r.GetByIdWithDetailsAsync(stock.Id, default))
                .ReturnsAsync(stock);
 
            mapperMock
                .Setup(m => m.Map(updateDTO, stock))
                .Callback(() => stock.Quantity = updateDTO.Quantity)
                .Returns(stock);
 
            mapperMock
                .Setup(m => m.Map<ClothesStockReadDTO>(stock))
                .Returns(ClothesStockFaker.GenerateReadDTO(stock.Id));
 
            await clothesStockService.UpdateAsync(stock.Id, updateDTO);
 
            stockNotificationServiceMock.Verify(
                s => s.NotifySubscribersAsync(stock.Id, default), Times.Once);
        }
 
        [Fact]
        public async Task UpdateAsync_WhenStockWasAlreadyPositive_DoesNotNotifySubscribers()
        {
            ClothesStock stock = ClothesStockFaker.GenerateStock(quantity: 3);
            ClothesStockUpdateDTO updateDTO = ClothesStockFaker.GenerateUpdateDTO(quantity: 10);
 
            stockRepoMock
                .Setup(r => r.GetByIdWithDetailsAsync(stock.Id, default))
                .ReturnsAsync(stock);
 
            mapperMock
                .Setup(m => m.Map(updateDTO, stock))
                .Callback(() => stock.Quantity = updateDTO.Quantity)
                .Returns(stock);
 
            mapperMock
                .Setup(m => m.Map<ClothesStockReadDTO>(stock))
                .Returns(ClothesStockFaker.GenerateReadDTO(stock.Id));
 
            await clothesStockService.UpdateAsync(stock.Id, updateDTO);
 
            stockNotificationServiceMock.Verify(
                s => s.NotifySubscribersAsync(It.IsAny<Guid>(), default), Times.Never);
        }
 
        [Fact]
        public async Task UpdateAsync_WhenSaveSucceeds_InvalidatesAllCaches()
        {
            ClothesStock stock = ClothesStockFaker.GenerateStock(quantity: 5);
            ClothesStockUpdateDTO updateDTO = ClothesStockFaker.GenerateUpdateDTO(quantity: 5);
 
            stockRepoMock
                .Setup(r => r.GetByIdWithDetailsAsync(stock.Id, default))
                .ReturnsAsync(stock);
 
            mapperMock.Setup(m => m.Map(updateDTO, stock)).Returns(stock);
            mapperMock.Setup(m => m.Map<ClothesStockReadDTO>(stock)).Returns(ClothesStockFaker.GenerateReadDTO(stock.Id));
 
            await clothesStockService.UpdateAsync(stock.Id, updateDTO);
 
            unitOfWorkMock.Verify(u => u.SaveChangesAsync(default), Times.Once);
            cacheInvalidationMock.Verify(c => c.InvalidateAllAsync(), Times.Once);
            cacheInvalidationMock.Verify(c => c.InvalidateByIdAsync(stock.Id), Times.Once);
            filterCacheInvalidationMock.Verify(c => c.InvalidateAsync(), Times.Once);
        }
    }