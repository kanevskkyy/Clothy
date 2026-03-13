using AutoMapper;
using Clothy.CatalogService.BLL.DTOs.BrandDTOs;
using Clothy.CatalogService.BLL.Interfaces;
using Clothy.CatalogService.BLL.Services;
using Clothy.CatalogService.DAL.UOW;
using Clothy.CatalogService.Domain.Entities.Catalog;
using Clothy.CatalogService.Domain.Entities.Clothe;
using Clothy.CatalogService.UnitTests.Helpers;
using Clothy.Shared.Cache.Interfaces;
using Clothy.Shared.Helpers.Exceptions;
using FluentAssertions;
using Moq;
using System.Diagnostics.Metrics;
using Clothy.Aggregator.Aggregate.RedisCache;
using Clothy.CatalogService.DAL.Interfaces;
using Xunit;

namespace Clothy.CatalogService.UnitTests.Services
{
    public class BrandServiceTests
    {
        private Mock<IUnitOfWork> unitOfWorkMock;
        private Mock<IBrandRepository> brandRepoMock;
        private Mock<IMapper> mapperMock;
        private Mock<IFilterCacheInvalidationService> filterCacheInvalidationMock;
        private Mock<IEntityCacheInvalidationService<Brand>> brandCacheInvalidationMock;
        private Mock<IEntityCacheInvalidationService<ClotheItem>> clotheItemCacheInvalidationMock;
        private Mock<IEntityCacheService> cacheServiceMock;
        
        private BrandService brandService;


        public BrandServiceTests()
        {
            unitOfWorkMock = new Mock<IUnitOfWork>();
            brandRepoMock = new Mock<IBrandRepository>();
            mapperMock = new Mock<IMapper>();
            filterCacheInvalidationMock = new Mock<IFilterCacheInvalidationService>();
            brandCacheInvalidationMock = new Mock<IEntityCacheInvalidationService<Brand>>();
            clotheItemCacheInvalidationMock = new Mock<IEntityCacheInvalidationService<ClotheItem>>();
            cacheServiceMock = new Mock<IEntityCacheService>();

            unitOfWorkMock
                .Setup(u => u.Brands)
                .Returns(brandRepoMock.Object);

            Meter meter = new Meter("test.brand.meter");

            brandService = new BrandService(
                unitOfWorkMock.Object,
                mapperMock.Object,
                filterCacheInvalidationMock.Object,
                meter,
                clotheItemCacheInvalidationMock.Object,
                brandCacheInvalidationMock.Object,
                cacheServiceMock.Object
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
        public async Task GetAllAsync_WhenBrandsExist_ReturnsListOfBrandReadDTOs()
        {
            List<Brand> brands = BrandFaker.GenerateBrands(3);
            List<BrandReadDTO> expectedDTOs = brands.Select(BrandFaker.GenerateReadDTOFromBrand).ToList();

            SetupCacheMiss<List<BrandReadDTO>?>();

            brandRepoMock
                .Setup(r => r.GetAllAsync(default))
                .ReturnsAsync(brands);

            mapperMock
                .Setup(m => m.Map<List<BrandReadDTO>>(brands))
                .Returns(expectedDTOs);

            List<BrandReadDTO>? result = await brandService.GetAllAsync();

            result.Should().NotBeNull();
            result.Should().HaveCount(3);
            result.Should().BeEquivalentTo(expectedDTOs);
        }

        [Fact]
        public async Task GetAllAsync_WhenNoBrandsExist_ReturnsEmptyList()
        {
            SetupCacheMiss<List<BrandReadDTO>?>();

            brandRepoMock
                .Setup(r => r.GetAllAsync(default))
                .ReturnsAsync(new List<Brand>());

            mapperMock
                .Setup(m => m.Map<List<BrandReadDTO>>(It.IsAny<List<Brand>>()))
                .Returns(new List<BrandReadDTO>());

            List<BrandReadDTO>? result = await brandService.GetAllAsync();
            
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }
        
        [Fact]
        public async Task GetByIdAsync_WhenBrandExists_ReturnsBrandReadDTO()
        {
            Brand brand = BrandFaker.GenerateBrand();
            BrandReadDTO expectedDTO = BrandFaker.GenerateReadDTOFromBrand(brand);

            SetupCacheMiss<BrandReadDTO?>();

            brandRepoMock
                .Setup(r => r.GetByIdAsync(brand.Id, default))
                .ReturnsAsync(brand);

            mapperMock
                .Setup(m => m.Map<BrandReadDTO>(brand))
                .Returns(expectedDTO);

            BrandReadDTO? result = await brandService.GetByIdAsync(brand.Id);

            result.Should().NotBeNull();
            result!.Id.Should().Be(brand.Id);
            result.Name.Should().Be(brand.Name);
        }

        [Fact]
        public async Task GetByIdAsync_WhenBrandNotFound_ThrowsNotFoundException()
        {
            Guid brandId = Guid.NewGuid();

            SetupCacheMiss<BrandReadDTO?>();

            brandRepoMock
                .Setup(r => r.GetByIdAsync(brandId, default))
                .ReturnsAsync((Brand?)null);

            var act = async () => await brandService.GetByIdAsync(brandId);

            await act.Should().ThrowAsync<NotFoundException>().WithMessage($"*{brandId}*");
        }


        [Fact]
        public async Task CreateAsync_WhenValidData_ReturnsBrandReadDTO()
        {
            BrandCreateDTO createDTO = BrandFaker.GenerateCreateDTO();
            Brand brand = BrandFaker.GenerateBrand();
            BrandReadDTO expectedDTO = BrandFaker.GenerateReadDTOFromBrand(brand);

            brandRepoMock
                .Setup(r => r.IsNameAlreadyExistsAsync(createDTO.Name, null, default))
                .ReturnsAsync(false);

            brandRepoMock
                .Setup(r => r.IsSlugAlreadyExistsAsync(createDTO.Slug, null, default))
                .ReturnsAsync(false);

            mapperMock
                .Setup(m => m.Map<Brand>(createDTO))
                .Returns(brand);

            mapperMock
                .Setup(m => m.Map<BrandReadDTO>(brand))
                .Returns(expectedDTO);

            BrandReadDTO result = await brandService.CreateAsync(createDTO);

            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(expectedDTO);
        }

        [Fact]
        public async Task CreateAsync_WhenValidData_SavesAndInvalidatesCache()
        {
            BrandCreateDTO createDTO = BrandFaker.GenerateCreateDTO();
            Brand brand = BrandFaker.GenerateBrand();

            brandRepoMock.Setup(r => r.IsNameAlreadyExistsAsync(createDTO.Name, null, default)).ReturnsAsync(false);
            brandRepoMock.Setup(r => r.IsSlugAlreadyExistsAsync(createDTO.Slug, null, default)).ReturnsAsync(false);
            mapperMock.Setup(m => m.Map<Brand>(createDTO)).Returns(brand);
            mapperMock.Setup(m => m.Map<BrandReadDTO>(brand)).Returns(BrandFaker.GenerateReadDTOFromBrand(brand));

            await brandService.CreateAsync(createDTO);
            
            unitOfWorkMock.Verify(u => u.SaveChangesAsync(default), Times.Once);
            brandCacheInvalidationMock.Verify(c => c.InvalidateAllAsync(), Times.Once);
            filterCacheInvalidationMock.Verify(c => c.InvalidateAsync(), Times.Once);
        }

        [Fact]
        public async Task CreateAsync_WhenNameAlreadyExists_ThrowsAlreadyExistsException()
        {
            BrandCreateDTO createDTO = BrandFaker.GenerateCreateDTO();

            brandRepoMock
                .Setup(r => r.IsNameAlreadyExistsAsync(createDTO.Name, null, default))
                .ReturnsAsync(true);

            var act = async () => await brandService.CreateAsync(createDTO);

            await act.Should().ThrowAsync<AlreadyExistsException>().WithMessage("*name*");

            unitOfWorkMock.Verify(u => u.SaveChangesAsync(default), Times.Never);
            brandCacheInvalidationMock.Verify(c => c.InvalidateAllAsync(), Times.Never);
        }

        [Fact]
        public async Task CreateAsync_WhenSlugAlreadyExists_ThrowsAlreadyExistsException()
        {
            BrandCreateDTO createDTO = BrandFaker.GenerateCreateDTO();

            brandRepoMock
                .Setup(r => r.IsNameAlreadyExistsAsync(createDTO.Name, null, default))
                .ReturnsAsync(false);

            brandRepoMock
                .Setup(r => r.IsSlugAlreadyExistsAsync(createDTO.Slug, null, default))
                .ReturnsAsync(true);

            var act = async () => await brandService.CreateAsync(createDTO);

            await act.Should().ThrowAsync<AlreadyExistsException>() 
                .WithMessage("*slug*");

            unitOfWorkMock.Verify(u => u.SaveChangesAsync(default), Times.Never);
        }
        
        [Fact]
        public async Task UpdateAsync_WhenValidData_ReturnsBrandReadDTO()
        {
            Brand brand = BrandFaker.GenerateBrand();
            BrandUpdateDTO updateDTO = BrandFaker.GenerateUpdateDTO();
            BrandReadDTO expectedDTO = BrandFaker.GenerateReadDTO(brand.Id);

            brandRepoMock
                .Setup(r => r.GetByIdAsync(brand.Id, default))
                .ReturnsAsync(brand);

            brandRepoMock
                .Setup(r => r.IsNameAlreadyExistsAsync(updateDTO.Name, brand.Id, default))
                .ReturnsAsync(false);

            brandRepoMock
                .Setup(r => r.IsSlugAlreadyExistsAsync(updateDTO.Slug, brand.Id, default))
                .ReturnsAsync(false);

            mapperMock
                .Setup(m => m.Map(updateDTO, brand))
                .Returns(brand);

            mapperMock
                .Setup(m => m.Map<BrandReadDTO>(brand))
                .Returns(expectedDTO);

            BrandReadDTO result = await brandService.UpdateAsync(brand.Id, updateDTO);

            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(expectedDTO);
        }

        [Fact]
        public async Task UpdateAsync_WhenValidData_SavesAndInvalidatesAllCaches()
        {
            Brand brand = BrandFaker.GenerateBrand();
            BrandUpdateDTO updateDTO = BrandFaker.GenerateUpdateDTO();

            brandRepoMock.Setup(r => r.GetByIdAsync(brand.Id, default)).ReturnsAsync(brand);
            brandRepoMock.Setup(r => r.IsNameAlreadyExistsAsync(updateDTO.Name, brand.Id, default)).ReturnsAsync(false);
            brandRepoMock.Setup(r => r.IsSlugAlreadyExistsAsync(updateDTO.Slug, brand.Id, default)).ReturnsAsync(false);
            mapperMock.Setup(m => m.Map(updateDTO, brand)).Returns(brand);
            mapperMock.Setup(m => m.Map<BrandReadDTO>(brand)).Returns(BrandFaker.GenerateReadDTO(brand.Id));

            await brandService.UpdateAsync(brand.Id, updateDTO);
            
            unitOfWorkMock.Verify(u => u.SaveChangesAsync(default), Times.Once);
            brandCacheInvalidationMock.Verify(c => c.InvalidateAllAsync(), Times.Once);
            brandCacheInvalidationMock.Verify(c => c.InvalidateByIdAsync(brand.Id), Times.Once);
            filterCacheInvalidationMock.Verify(c => c.InvalidateAsync(), Times.Once);
            clotheItemCacheInvalidationMock.Verify(c => c.InvalidateAllAsync(), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_WhenBrandNotFound_ThrowsNotFoundException()
        {
            Guid brandId = Guid.NewGuid();
            BrandUpdateDTO updateDTO = BrandFaker.GenerateUpdateDTO();

            brandRepoMock
                .Setup(r => r.GetByIdAsync(brandId, default))
                .ReturnsAsync((Brand?)null);

            var act = async () => await brandService.UpdateAsync(brandId, updateDTO);
            
            await act.Should().ThrowAsync<NotFoundException>()
                .WithMessage($"*{brandId}*");

            unitOfWorkMock.Verify(u => u.SaveChangesAsync(default), Times.Never);
        }

        [Fact]
        public async Task UpdateAsync_WhenNameAlreadyExists_ThrowsAlreadyExistsException()
        {
            Brand brand = BrandFaker.GenerateBrand();
            BrandUpdateDTO updateDTO = BrandFaker.GenerateUpdateDTO();

            brandRepoMock.Setup(r => r.GetByIdAsync(brand.Id, default)).ReturnsAsync(brand);
            brandRepoMock
                .Setup(r => r.IsNameAlreadyExistsAsync(updateDTO.Name, brand.Id, default))
                .ReturnsAsync(true);

            var act = async () => await brandService.UpdateAsync(brand.Id, updateDTO);

            await act.Should().ThrowAsync<AlreadyExistsException>()
                .WithMessage("*name*");

            unitOfWorkMock.Verify(u => u.SaveChangesAsync(default), Times.Never);
        }

        [Fact]
        public async Task UpdateAsync_WhenSlugAlreadyExists_ThrowsAlreadyExistsException()
        {
            Brand brand = BrandFaker.GenerateBrand();
            BrandUpdateDTO updateDTO = BrandFaker.GenerateUpdateDTO();

            brandRepoMock.Setup(r => r.GetByIdAsync(brand.Id, default)).ReturnsAsync(brand);
            brandRepoMock.Setup(r => r.IsNameAlreadyExistsAsync(updateDTO.Name, brand.Id, default)).ReturnsAsync(false);
            brandRepoMock
                .Setup(r => r.IsSlugAlreadyExistsAsync(updateDTO.Slug, brand.Id, default))
                .ReturnsAsync(true);

            var act = async () => await brandService.UpdateAsync(brand.Id, updateDTO);

            await act.Should().ThrowAsync<AlreadyExistsException>()
                .WithMessage("*slug*");

            unitOfWorkMock.Verify(u => u.SaveChangesAsync(default), Times.Never);
        }
        
        [Fact]
        public async Task DeleteAsync_WhenBrandExists_DeletesSuccessfully()
        {
            Brand brand = BrandFaker.GenerateBrand();

            brandRepoMock
                .Setup(r => r.GetByIdAsync(brand.Id, default))
                .ReturnsAsync(brand);

            await brandService.DeleteAsync(brand.Id);

            brandRepoMock.Verify(r => r.Delete(brand), Times.Once);
            unitOfWorkMock.Verify(u => u.SaveChangesAsync(default), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_WhenBrandExists_InvalidatesAllCaches()
        {
            Brand brand = BrandFaker.GenerateBrand();

            brandRepoMock
                .Setup(r => r.GetByIdAsync(brand.Id, default))
                .ReturnsAsync(brand);

            await brandService.DeleteAsync(brand.Id);

            brandCacheInvalidationMock.Verify(c => c.InvalidateByIdAsync(brand.Id), Times.Once);
            brandCacheInvalidationMock.Verify(c => c.InvalidateAllAsync(), Times.Once);
            filterCacheInvalidationMock.Verify(c => c.InvalidateAsync(), Times.Once);
            clotheItemCacheInvalidationMock.Verify(c => c.InvalidateAllAsync(), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_WhenBrandNotFound_ThrowsNotFoundException()
        {
            Guid brandId = Guid.NewGuid();

            brandRepoMock
                .Setup(r => r.GetByIdAsync(brandId, default))
                .ReturnsAsync((Brand?)null);

            var act = async () => await brandService.DeleteAsync(brandId);

            await act.Should().ThrowAsync<NotFoundException>()
                .WithMessage($"*{brandId}*");

            brandRepoMock.Verify(r => r.Delete(It.IsAny<Brand>()), Times.Never);
            unitOfWorkMock.Verify(u => u.SaveChangesAsync(default), Times.Never);
        }
    }
}