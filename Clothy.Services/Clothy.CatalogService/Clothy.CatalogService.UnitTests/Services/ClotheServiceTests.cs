using AutoMapper;
using Clothy.CatalogService.BLL.DTOs.ClotheDTOs;
using Clothy.CatalogService.BLL.Exceptions;
using Clothy.CatalogService.BLL.Services;
using Clothy.CatalogService.DAL.UOW;
using Clothy.CatalogService.Domain.Entities.Catalog;
using Clothy.CatalogService.Domain.Entities.Clothe;
using Clothy.CatalogService.Domain.Entities.Stock;
using Clothy.CatalogService.Domain.QueryParameters;
using Clothy.CatalogService.UnitTests.Helpers;
using Clothy.Shared.Cache.Interfaces;
using Clothy.Shared.Events.ClotheItemEvents;
using Clothy.Shared.Helpers;
using Clothy.Shared.Helpers.CloudinaryConfig.ImageService;
using Clothy.Shared.Helpers.Exceptions;
using FluentAssertions;
using MassTransit;
using Moq;
using System.Diagnostics.Metrics;
using Clothy.Aggregator.Aggregate.RedisCache;
using Clothy.CatalogService.BLL.DTOs.MaterialDTOs;
using Clothy.CatalogService.BLL.DTOs.PhotoDTOs;
using Clothy.CatalogService.DAL.Interfaces;
using Xunit;

namespace Clothy.CatalogService.UnitTests.Services;

public class ClotheServiceTests
    {
        private Mock<IUnitOfWork> unitOfWorkMock;
        private Mock<IClotheItemRepository> clotheItemRepoMock;
        private Mock<IBrandRepository> brandRepoMock;
        private Mock<IClothingTypeRepository> clothingTypeRepoMock;
        private Mock<ICollectionRepository> collectionRepoMock;
        private Mock<ISizeRepository> sizeRepoMock;
        private Mock<IColorRepository> colorRepoMock;
        private Mock<IMaterialRepository> materialRepoMock;
        private Mock<ITagRepository> tagRepoMock;
        private Mock<IClothePopularityRepository> popularityRepoMock;
        private Mock<IMapper> mapperMock;
        private Mock<IImageService> imageServiceMock;
        private Mock<IEntityCacheService> cacheServiceMock;
        private Mock<IEntityCacheInvalidationService<ClotheItem>> clotheItemCacheInvalidationMock;
        private Mock<IEntityCacheInvalidationService<ClothesStock>> clotheStockCacheInvalidationMock;
        private Mock<IPublishEndpoint> publishEndpointMock;
        private Mock<IFilterCacheInvalidationService> filterCacheInvalidationMock;
 
        private ClotheService clotheService;
 
        public ClotheServiceTests()
        {
            unitOfWorkMock = new Mock<IUnitOfWork>();
            clotheItemRepoMock = new Mock<IClotheItemRepository>();
            brandRepoMock = new Mock<IBrandRepository>();
            clothingTypeRepoMock = new Mock<IClothingTypeRepository>();
            collectionRepoMock = new Mock<ICollectionRepository>();
            sizeRepoMock = new Mock<ISizeRepository>();
            colorRepoMock = new Mock<IColorRepository>();
            materialRepoMock = new Mock<IMaterialRepository>();
            tagRepoMock = new Mock<ITagRepository>();
            popularityRepoMock = new Mock<IClothePopularityRepository>();
            mapperMock = new Mock<IMapper>();
            imageServiceMock = new Mock<IImageService>();
            cacheServiceMock = new Mock<IEntityCacheService>();
            clotheItemCacheInvalidationMock = new Mock<IEntityCacheInvalidationService<ClotheItem>>();
            clotheStockCacheInvalidationMock = new Mock<IEntityCacheInvalidationService<ClothesStock>>();
            publishEndpointMock = new Mock<IPublishEndpoint>();
            filterCacheInvalidationMock = new Mock<IFilterCacheInvalidationService>();
 
            unitOfWorkMock.Setup(u => u.ClotheItems).Returns(clotheItemRepoMock.Object);
            unitOfWorkMock.Setup(u => u.Brands).Returns(brandRepoMock.Object);
            unitOfWorkMock.Setup(u => u.ClothingTypes).Returns(clothingTypeRepoMock.Object);
            unitOfWorkMock.Setup(u => u.Collections).Returns(collectionRepoMock.Object);
            unitOfWorkMock.Setup(u => u.Sizes).Returns(sizeRepoMock.Object);
            unitOfWorkMock.Setup(u => u.Colors).Returns(colorRepoMock.Object);
            unitOfWorkMock.Setup(u => u.Materials).Returns(materialRepoMock.Object);
            unitOfWorkMock.Setup(u => u.Tags).Returns(tagRepoMock.Object);
            unitOfWorkMock.Setup(u => u.ClothePopularity).Returns(popularityRepoMock.Object);
 
            Meter meter = new Meter("test.clothe.meter");
 
            clotheService = new ClotheService(
                unitOfWorkMock.Object,
                mapperMock.Object,
                imageServiceMock.Object,
                cacheServiceMock.Object,
                clotheItemCacheInvalidationMock.Object,
                meter,
                clotheStockCacheInvalidationMock.Object,
                publishEndpointMock.Object,
                filterCacheInvalidationMock.Object
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
        public async Task GetPagedClotheItemsAsync_WhenPageWithinCacheLimit_UsesCacheLayer()
        {
            ClotheItemSpecificationParameters parameters = ClotheItemFaker.GenerateParameters(pageNumber: 1);
            List<ClotheSummaryDTO> dtos = ClotheItemFaker.GenerateClotheSummaryDTOs(3);
            PagedList<ClotheSummaryDTO> pagedResult = new PagedList<ClotheSummaryDTO>(dtos, 3, 1, 10);
 
            cacheServiceMock
                .Setup(c => c.GetOrSetAsync(
                    It.IsAny<string>(),
                    It.IsAny<Func<Task<PagedList<ClotheSummaryDTO>?>>>(),
                    It.IsAny<TimeSpan>(),
                    It.IsAny<TimeSpan>()))
                .ReturnsAsync(pagedResult);
 
            PagedList<ClotheSummaryDTO>? result = await clotheService.GetPagedClotheItemsAsync(parameters);
 
            result.Should().NotBeNull();
            cacheServiceMock.Verify(c => c.GetOrSetAsync(
                It.IsAny<string>(),
                It.IsAny<Func<Task<PagedList<ClotheSummaryDTO>?>>>(),
                It.IsAny<TimeSpan>(),
                It.IsAny<TimeSpan>()), Times.Once);
        }
 
        [Fact]
        public async Task GetPagedClotheItemsAsync_WhenPageExceedsCacheLimit_SkipsCacheLayer()
        {
            ClotheItemSpecificationParameters parameters = ClotheItemFaker.GenerateParameters(pageNumber: 11);
            List<ClotheItem> entities = new List<ClotheItem> { ClotheItemFaker.GenerateClotheItem() };
            List<ClotheSummaryDTO> dtos = ClotheItemFaker.GenerateClotheSummaryDTOs(1);
            PagedList<ClotheItem> paged = new PagedList<ClotheItem>(entities, 1, 11, 10);
 
            clotheItemRepoMock
                .Setup(r => r.GetPagedClotheItemsAsync(parameters, default))
                .ReturnsAsync(paged);
 
            mapperMock
                .Setup(m => m.Map<List<ClotheSummaryDTO>>(paged.Items))
                .Returns(dtos);
 
            PagedList<ClotheSummaryDTO>? result = await clotheService.GetPagedClotheItemsAsync(parameters);
 
            result.Should().NotBeNull();
            cacheServiceMock.Verify(c => c.GetOrSetAsync(
                It.IsAny<string>(),
                It.IsAny<Func<Task<PagedList<ClotheSummaryDTO>?>>>(),
                It.IsAny<TimeSpan>(),
                It.IsAny<TimeSpan>()), Times.Never);
        }
        
        [Fact]
        public async Task GetDetailBySlugAsync_WhenItemExists_ReturnsClotheDetailDTO()
        {
            ClotheItem clotheItem = ClotheItemFaker.GenerateClotheItem();
            ClotheDetailDTO expectedDTO = ClotheItemFaker.GenerateClotheDetailDTO(clotheItem.Id, clotheItem.Slug);
 
            SetupCacheMiss<ClotheDetailDTO?>();
 
            clotheItemRepoMock
                .Setup(r => r.GetBySlugWithDetailsAsync(clotheItem.Slug!, default))
                .ReturnsAsync(clotheItem);
 
            mapperMock
                .Setup(m => m.Map<ClotheDetailDTO>(clotheItem))
                .Returns(expectedDTO);
 
            ClotheDetailDTO result = await clotheService.GetDetailBySlugAsync(clotheItem.Slug!);
 
            result.Should().NotBeNull();
            result.Id.Should().Be(clotheItem.Id);
            result.Slug.Should().Be(clotheItem.Slug);
        }
 
        [Fact]
        public async Task GetDetailBySlugAsync_WhenItemNotFound_ThrowsNotFoundException()
        {
            string slug = "non-existent-slug";
 
            SetupCacheMiss<ClotheDetailDTO?>();
 
            clotheItemRepoMock
                .Setup(r => r.GetBySlugWithDetailsAsync(slug, default))
                .ReturnsAsync((ClotheItem?)null);
 
            Func<Task> act = async () => await clotheService.GetDetailBySlugAsync(slug);
 
            await act.Should().ThrowAsync<NotFoundException>()
                .WithMessage($"*{slug}*");
        }
        
        [Fact]
        public async Task GetDetailByIdAsync_WhenItemExists_ReturnsClotheDetailDTO()
        {
            ClotheItem clotheItem = ClotheItemFaker.GenerateClotheItem();
            ClotheDetailDTO expectedDTO = ClotheItemFaker.GenerateClotheDetailDTO(clotheItem.Id);
 
            clotheItemRepoMock
                .Setup(r => r.GetByIdWithDetailsAsync(clotheItem.Id, default))
                .ReturnsAsync(clotheItem);
 
            mapperMock
                .Setup(m => m.Map<ClotheDetailDTO>(clotheItem))
                .Returns(expectedDTO);
 
            ClotheDetailDTO result = await clotheService.GetDetailByIdAsync(clotheItem.Id);
 
            result.Should().NotBeNull();
            result.Id.Should().Be(clotheItem.Id);
        }
 
        [Fact]
        public async Task GetDetailByIdAsync_WhenItemNotFound_ThrowsNotFoundException()
        {
            Guid id = Guid.NewGuid();
 
            clotheItemRepoMock
                .Setup(r => r.GetByIdWithDetailsAsync(id, default))
                .ReturnsAsync((ClotheItem?)null);
 
            Func<Task> act = async () => await clotheService.GetDetailByIdAsync(id);
 
            await act.Should().ThrowAsync<NotFoundException>()
                .WithMessage($"*{id}*");
        }
 
        [Fact]
        public async Task GetMinAndMaxPriceAsync_ReturnsPriceRangeDTO()
        {
            clotheItemRepoMock
                .Setup(r => r.GetMinAndMaxPriceAsync(default))
                .ReturnsAsync((10m, 500m));
 
            PriceRangeDTO result = await clotheService.GetMinAndMaxPriceAsync();
 
            result.Should().NotBeNull();
            result.MinPrice.Should().Be(10m);
            result.MaxPrice.Should().Be(500m);
        }
        
        [Fact]
        public async Task GetTop8MostPopularAsync_ReturnsMappedDTOs()
        {
            List<ClotheItem> entities = Enumerable.Range(0, 8)
                .Select(_ => ClotheItemFaker.GenerateClotheItem())
                .ToList();
 
            List<ClotheSummaryDTO> dtos = ClotheItemFaker.GenerateClotheSummaryDTOs(8);
 
            SetupCacheMiss<List<ClotheSummaryDTO>?>();
 
            popularityRepoMock
                .Setup(r => r.GetTop8MostPopularAsync(default))
                .ReturnsAsync(entities);
 
            mapperMock
                .Setup(m => m.Map<List<ClotheSummaryDTO>>(entities))
                .Returns(dtos);
 
            List<ClotheSummaryDTO>? result = await clotheService.GetTop8MostPopularAsync();
 
            result.Should().NotBeNull();
            result.Should().HaveCount(8);
        }
        
        [Fact]
        public async Task CreateAsync_WhenSlugAlreadyExists_ThrowsAlreadyExistsException()
        {
            ClotheCreateDTO createDTO = ClotheItemFaker.GenerateCreateDTO();
 
            clotheItemRepoMock
                .Setup(r => r.IsSlugAlreadyExistsAsync(createDTO.Slug, null, default))
                .ReturnsAsync(true);
 
            Func<Task> act = async () => await clotheService.CreateAsync(createDTO);
 
            await act.Should().ThrowAsync<AlreadyExistsException>()
                .WithMessage("*slug*");
 
            unitOfWorkMock.Verify(u => u.SaveChangesAsync(default), Times.Never);
        }
 
        [Fact]
        public async Task CreateAsync_WhenMaterialPercentageNot100_ThrowsInvalidMaterialPercentageException()
        {
            List<ClotheMaterialCreateDTO> badMaterials = new List<ClotheMaterialCreateDTO>
            {
                new ClotheMaterialCreateDTO { MaterialId = Guid.NewGuid(), Percentage = 60 },
                new ClotheMaterialCreateDTO { MaterialId = Guid.NewGuid(), Percentage = 20 }
            };
 
            ClotheCreateDTO createDTO = ClotheItemFaker.GenerateCreateDTO(materials: badMaterials);
 
            clotheItemRepoMock
                .Setup(r => r.IsSlugAlreadyExistsAsync(createDTO.Slug, null, default))
                .ReturnsAsync(false);
 
            Func<Task> act = async () => await clotheService.CreateAsync(createDTO);
 
            await act.Should().ThrowAsync<InvalidMaterialPercentageException>()
                .WithMessage("*100*");
 
            unitOfWorkMock.Verify(u => u.SaveChangesAsync(default), Times.Never);
        }
 
        [Fact]
        public async Task CreateAsync_WhenBrandNotFound_ThrowsNotFoundException()
        {
            ClotheCreateDTO createDTO = ClotheItemFaker.GenerateCreateDTO();
 
            clotheItemRepoMock
                .Setup(r => r.IsSlugAlreadyExistsAsync(createDTO.Slug, null, default))
                .ReturnsAsync(false);
 
            brandRepoMock
                .Setup(r => r.GetByIdAsync(createDTO.BrandId, default))
                .ReturnsAsync((Brand?)null);
 
            Func<Task> act = async () => await clotheService.CreateAsync(createDTO);
 
            await act.Should().ThrowAsync<NotFoundException>()
                .WithMessage($"*{createDTO.BrandId}*");
 
            unitOfWorkMock.Verify(u => u.SaveChangesAsync(default), Times.Never);
        }
 
        [Fact]
        public async Task CreateAsync_WhenClothingTypeNotFound_ThrowsNotFoundException()
        {
            ClotheCreateDTO createDTO = ClotheItemFaker.GenerateCreateDTO();
 
            clotheItemRepoMock
                .Setup(r => r.IsSlugAlreadyExistsAsync(createDTO.Slug, null, default))
                .ReturnsAsync(false);
 
            brandRepoMock
                .Setup(r => r.GetByIdAsync(createDTO.BrandId, default))
                .ReturnsAsync(new Brand { Id = createDTO.BrandId, Name = "TestBrand" });
 
            clothingTypeRepoMock
                .Setup(r => r.GetByIdAsync(createDTO.ClothingTypeId, default))
                .ReturnsAsync((ClothingType?)null);
 
            Func<Task> act = async () => await clotheService.CreateAsync(createDTO);
 
            await act.Should().ThrowAsync<NotFoundException>()
                .WithMessage($"*{createDTO.ClothingTypeId}*");
 
            unitOfWorkMock.Verify(u => u.SaveChangesAsync(default), Times.Never);
        }
 
        [Fact]
        public async Task CreateAsync_WhenCollectionNotFound_ThrowsNotFoundException()
        {
            ClotheCreateDTO createDTO = ClotheItemFaker.GenerateCreateDTO();
 
            clotheItemRepoMock
                .Setup(r => r.IsSlugAlreadyExistsAsync(createDTO.Slug, null, default))
                .ReturnsAsync(false);
 
            brandRepoMock
                .Setup(r => r.GetByIdAsync(createDTO.BrandId, default))
                .ReturnsAsync(new Brand { Id = createDTO.BrandId, Name = "TestBrand" });
 
            clothingTypeRepoMock
                .Setup(r => r.GetByIdAsync(createDTO.ClothingTypeId, default))
                .ReturnsAsync(new ClothingType { Id = createDTO.ClothingTypeId, Name = "TestType" });
 
            collectionRepoMock
                .Setup(r => r.GetByIdAsync(createDTO.CollectionId, default))
                .ReturnsAsync((Collection?)null);
 
            Func<Task> act = async () => await clotheService.CreateAsync(createDTO);
 
            await act.Should().ThrowAsync<NotFoundException>()
                .WithMessage($"*{createDTO.CollectionId}*");
 
            unitOfWorkMock.Verify(u => u.SaveChangesAsync(default), Times.Never);
        }
 
        [Fact]
        public async Task CreateAsync_WhenDuplicateMainPhotoPerColor_ThrowsValidationFailedException()
        {
            Guid colorId = Guid.NewGuid();
 
            List<ClothePhotoCreateDTO> duplicateMainPhotos = new List<ClothePhotoCreateDTO>
            {
                new ClothePhotoCreateDTO { ColorId = colorId, IsMain = true, Photo = new Mock<Microsoft.AspNetCore.Http.IFormFile>().Object },
                new ClothePhotoCreateDTO { ColorId = colorId, IsMain = true, Photo = new Mock<Microsoft.AspNetCore.Http.IFormFile>().Object }
            };
 
            ClotheCreateDTO createDTO = ClotheItemFaker.GenerateCreateDTO(photos: duplicateMainPhotos);
 
            clotheItemRepoMock
                .Setup(r => r.IsSlugAlreadyExistsAsync(createDTO.Slug, null, default))
                .ReturnsAsync(false);
 
            brandRepoMock
                .Setup(r => r.GetByIdAsync(createDTO.BrandId, default))
                .ReturnsAsync(new Brand { Id = createDTO.BrandId, Name = "TestBrand" });
 
            clothingTypeRepoMock
                .Setup(r => r.GetByIdAsync(createDTO.ClothingTypeId, default))
                .ReturnsAsync(new ClothingType { Id = createDTO.ClothingTypeId, Name = "TestType" });
 
            collectionRepoMock
                .Setup(r => r.GetByIdAsync(createDTO.CollectionId, default))
                .ReturnsAsync(new Collection { Id = createDTO.CollectionId, Name = "TestCollection" });
 
            mapperMock
                .Setup(m => m.Map<ClotheItem>(createDTO))
                .Returns(ClotheItemFaker.GenerateClotheItem());
 
            Func<Task> act = async () => await clotheService.CreateAsync(createDTO);
 
            await act.Should().ThrowAsync<ValidationFailedException>();
 
            unitOfWorkMock.Verify(u => u.SaveChangesAsync(default), Times.Never);
        }
 
        [Fact]
        public async Task UpdateAsync_WhenItemNotFound_ThrowsNotFoundException()
        {
            Guid id = Guid.NewGuid();
            ClotheUpdateDTO updateDTO = ClotheItemFaker.GenerateUpdateDTO();
 
            clotheItemRepoMock
                .Setup(r => r.GetByIdAsync(id, default))
                .ReturnsAsync((ClotheItem?)null);
 
            Func<Task> act = async () => await clotheService.UpdateAsync(id, updateDTO);
 
            await act.Should().ThrowAsync<NotFoundException>()
                .WithMessage($"*{id}*");
 
            unitOfWorkMock.Verify(u => u.SaveChangesAsync(default), Times.Never);
        }
 
        [Fact]
        public async Task UpdateAsync_WhenSlugAlreadyExists_ThrowsAlreadyExistsException()
        {
            ClotheItem clotheItem = ClotheItemFaker.GenerateClotheItem();
            ClotheUpdateDTO updateDTO = ClotheItemFaker.GenerateUpdateDTO();
 
            clotheItemRepoMock
                .Setup(r => r.GetByIdAsync(clotheItem.Id, default))
                .ReturnsAsync(clotheItem);
 
            clotheItemRepoMock
                .Setup(r => r.IsSlugAlreadyExistsAsync(updateDTO.Slug, clotheItem.Id, default))
                .ReturnsAsync(true);
 
            Func<Task> act = async () => await clotheService.UpdateAsync(clotheItem.Id, updateDTO);
 
            await act.Should().ThrowAsync<AlreadyExistsException>()
                .WithMessage("*slug*");
 
            unitOfWorkMock.Verify(u => u.SaveChangesAsync(default), Times.Never);
        }
 
        [Fact]
        public async Task UpdateAsync_WhenBrandNotFound_ThrowsNotFoundException()
        {
            ClotheItem clotheItem = ClotheItemFaker.GenerateClotheItem();
            ClotheUpdateDTO updateDTO = ClotheItemFaker.GenerateUpdateDTO();
 
            clotheItemRepoMock.Setup(r => r.GetByIdAsync(clotheItem.Id, default)).ReturnsAsync(clotheItem);
            clotheItemRepoMock.Setup(r => r.IsSlugAlreadyExistsAsync(updateDTO.Slug, clotheItem.Id, default)).ReturnsAsync(false);
 
            brandRepoMock
                .Setup(r => r.GetByIdAsync(updateDTO.BrandId, default))
                .ReturnsAsync((Brand?)null);
 
            Func<Task> act = async () => await clotheService.UpdateAsync(clotheItem.Id, updateDTO);
 
            await act.Should().ThrowAsync<NotFoundException>()
                .WithMessage($"*{updateDTO.BrandId}*");
 
            unitOfWorkMock.Verify(u => u.SaveChangesAsync(default), Times.Never);
        }
 
        [Fact]
        public async Task UpdateAsync_WhenPriceDecreased_SetsOldPrice()
        {
            ClotheItem clotheItem = ClotheItemFaker.GenerateClotheItem();
            clotheItem.Price = 200m;
            clotheItem.OldPrice = null;
 
            ClotheUpdateDTO updateDTO = ClotheItemFaker.GenerateUpdateDTO();
            updateDTO.Price = 100m;
 
            ClotheDetailDTO detailDTO = ClotheItemFaker.GenerateClotheDetailDTO(clotheItem.Id, clotheItem.Slug);
 
            clotheItemRepoMock.Setup(r => r.GetByIdAsync(clotheItem.Id, default)).ReturnsAsync(clotheItem);
            clotheItemRepoMock.Setup(r => r.IsSlugAlreadyExistsAsync(updateDTO.Slug, clotheItem.Id, default)).ReturnsAsync(false);
 
            brandRepoMock
                .Setup(r => r.GetByIdAsync(updateDTO.BrandId, default))
                .ReturnsAsync(new Brand { Id = updateDTO.BrandId, Name = "TestBrand" });
 
            clothingTypeRepoMock
                .Setup(r => r.GetByIdAsync(updateDTO.ClothingTypeId, default))
                .ReturnsAsync(new ClothingType { Id = updateDTO.ClothingTypeId, Name = "TestType" });
 
            collectionRepoMock
                .Setup(r => r.GetByIdAsync(updateDTO.CollectionId, default))
                .ReturnsAsync(new Collection { Id = updateDTO.CollectionId, Name = "TestCollection" });
 
            mapperMock
                .Setup(m => m.Map(updateDTO, clotheItem))
                .Callback(() => clotheItem.Price = updateDTO.Price)
                .Returns(clotheItem);
 
            SetupCacheMiss<ClotheDetailDTO?>();
 
            clotheItemRepoMock
                .Setup(r => r.GetBySlugWithDetailsAsync(clotheItem.Slug!, default))
                .ReturnsAsync(clotheItem);
 
            mapperMock
                .Setup(m => m.Map<ClotheDetailDTO>(clotheItem))
                .Returns(detailDTO);
 
            await clotheService.UpdateAsync(clotheItem.Id, updateDTO);
 
            clotheItem.OldPrice.Should().Be(200m);
        }
 
        [Fact]
        public async Task UpdateAsync_WhenPriceIncreasedAboveOldPrice_ClearsOldPrice()
        {
            ClotheItem clotheItem = ClotheItemFaker.GenerateClotheItem();
            clotheItem.Price = 100m;
            clotheItem.OldPrice = 150m;
 
            ClotheUpdateDTO updateDTO = ClotheItemFaker.GenerateUpdateDTO();
            updateDTO.Price = 200m;
 
            ClotheDetailDTO detailDTO = ClotheItemFaker.GenerateClotheDetailDTO(clotheItem.Id, clotheItem.Slug);
 
            clotheItemRepoMock.Setup(r => r.GetByIdAsync(clotheItem.Id, default)).ReturnsAsync(clotheItem);
            clotheItemRepoMock.Setup(r => r.IsSlugAlreadyExistsAsync(updateDTO.Slug, clotheItem.Id, default)).ReturnsAsync(false);
 
            brandRepoMock
                .Setup(r => r.GetByIdAsync(updateDTO.BrandId, default))
                .ReturnsAsync(new Brand { Id = updateDTO.BrandId, Name = "TestBrand" });
 
            clothingTypeRepoMock
                .Setup(r => r.GetByIdAsync(updateDTO.ClothingTypeId, default))
                .ReturnsAsync(new ClothingType { Id = updateDTO.ClothingTypeId, Name = "TestType" });
 
            collectionRepoMock
                .Setup(r => r.GetByIdAsync(updateDTO.CollectionId, default))
                .ReturnsAsync(new Collection { Id = updateDTO.CollectionId, Name = "TestCollection" });
 
            mapperMock
                .Setup(m => m.Map(updateDTO, clotheItem))
                .Callback(() => clotheItem.Price = updateDTO.Price)
                .Returns(clotheItem);
 
            SetupCacheMiss<ClotheDetailDTO?>();
 
            clotheItemRepoMock
                .Setup(r => r.GetBySlugWithDetailsAsync(clotheItem.Slug!, default))
                .ReturnsAsync(clotheItem);
 
            mapperMock
                .Setup(m => m.Map<ClotheDetailDTO>(clotheItem))
                .Returns(detailDTO);
 
            await clotheService.UpdateAsync(clotheItem.Id, updateDTO);
 
            clotheItem.OldPrice.Should().BeNull();
        }
 
        [Fact]
        public async Task UpdateAsync_WhenSaveSucceeds_PublishesClotheItemUpdatedEvent()
        {
            ClotheItem clotheItem = ClotheItemFaker.GenerateClotheItem();
            clotheItem.Price = 100m;
            ClotheUpdateDTO updateDTO = ClotheItemFaker.GenerateUpdateDTO();
            updateDTO.Price = 100m;
 
            ClotheDetailDTO detailDTO = ClotheItemFaker.GenerateClotheDetailDTO(clotheItem.Id, clotheItem.Slug);
 
            clotheItemRepoMock.Setup(r => r.GetByIdAsync(clotheItem.Id, default)).ReturnsAsync(clotheItem);
            clotheItemRepoMock.Setup(r => r.IsSlugAlreadyExistsAsync(updateDTO.Slug, clotheItem.Id, default)).ReturnsAsync(false);
 
            brandRepoMock
                .Setup(r => r.GetByIdAsync(updateDTO.BrandId, default))
                .ReturnsAsync(new Brand { Id = updateDTO.BrandId, Name = "TestBrand" });
 
            clothingTypeRepoMock
                .Setup(r => r.GetByIdAsync(updateDTO.ClothingTypeId, default))
                .ReturnsAsync(new ClothingType { Id = updateDTO.ClothingTypeId, Name = "TestType" });
 
            collectionRepoMock
                .Setup(r => r.GetByIdAsync(updateDTO.CollectionId, default))
                .ReturnsAsync(new Collection { Id = updateDTO.CollectionId, Name = "TestCollection" });
 
            mapperMock.Setup(m => m.Map(updateDTO, clotheItem)).Returns(clotheItem);
 
            SetupCacheMiss<ClotheDetailDTO?>();
 
            clotheItemRepoMock
                .Setup(r => r.GetBySlugWithDetailsAsync(clotheItem.Slug!, default))
                .ReturnsAsync(clotheItem);
 
            mapperMock.Setup(m => m.Map<ClotheDetailDTO>(clotheItem)).Returns(detailDTO);
 
            await clotheService.UpdateAsync(clotheItem.Id, updateDTO);
 
            publishEndpointMock.Verify(p => p.Publish(
                It.Is<ClotheItemUpdatedEvent>(e => e.ClotheId == clotheItem.Id),
                default), Times.Once);
        }
        
        [Fact]
        public async Task DeleteAsync_WhenItemNotFound_ThrowsNotFoundException()
        {
            Guid id = Guid.NewGuid();
 
            clotheItemRepoMock
                .Setup(r => r.GetByIdWithDetailsAsync(id, default))
                .ReturnsAsync((ClotheItem?)null);
 
            Func<Task> act = async () => await clotheService.DeleteAsync(id);
 
            await act.Should().ThrowAsync<NotFoundException>()
                .WithMessage($"*{id}*");
 
            unitOfWorkMock.Verify(u => u.SaveChangesAsync(default), Times.Never);
        }
 
        [Fact]
        public async Task DeleteAsync_WhenItemExists_DeletesPhotosFromStorage()
        {
            ClotheItem clotheItem = ClotheItemFaker.GenerateClotheItem();
            clotheItem.Photos = new List<PhotoClothes>
            {
                new PhotoClothes { PhotoURL = "https://cloudinary.com/photo1.jpg" },
                new PhotoClothes { PhotoURL = "https://cloudinary.com/photo2.jpg" }
            };
 
            clotheItemRepoMock
                .Setup(r => r.GetByIdWithDetailsAsync(clotheItem.Id, default))
                .ReturnsAsync(clotheItem);
 
            await clotheService.DeleteAsync(clotheItem.Id);
 
            imageServiceMock.Verify(s => s.DeleteImageAsync(It.IsAny<string>()), Times.Exactly(2));
        }
 
        [Fact]
        public async Task DeleteAsync_WhenItemExists_SavesAndInvalidatesAllCaches()
        {
            ClotheItem clotheItem = ClotheItemFaker.GenerateClotheItem();
            clotheItem.Photos = new List<PhotoClothes>();
 
            clotheItemRepoMock
                .Setup(r => r.GetByIdWithDetailsAsync(clotheItem.Id, default))
                .ReturnsAsync(clotheItem);
 
            await clotheService.DeleteAsync(clotheItem.Id);
 
            unitOfWorkMock.Verify(u => u.SaveChangesAsync(default), Times.Once);
            clotheItemCacheInvalidationMock.Verify(c => c.InvalidateAllAsync(), Times.Once);
            clotheStockCacheInvalidationMock.Verify(c => c.InvalidateAllAsync(), Times.Once);
            filterCacheInvalidationMock.Verify(c => c.InvalidateAsync(), Times.Once);
        }
 
        [Fact]
        public async Task DeleteAsync_WhenItemExists_PublishesClotheItemDeletedEvent()
        {
            ClotheItem clotheItem = ClotheItemFaker.GenerateClotheItem();
            clotheItem.Photos = new List<PhotoClothes>();
 
            clotheItemRepoMock
                .Setup(r => r.GetByIdWithDetailsAsync(clotheItem.Id, default))
                .ReturnsAsync(clotheItem);
 
            await clotheService.DeleteAsync(clotheItem.Id);
 
            publishEndpointMock.Verify(p => p.Publish(
                It.Is<ClotheItemDeletedEvent>(e => e.ClotheId == clotheItem.Id),
                default), Times.Once);
        }
    }