using Bogus;
using Clothy.CatalogService.BLL.DTOs.BrandDTOs;
using Clothy.CatalogService.BLL.DTOs.ClotheDTOs;
using Clothy.CatalogService.BLL.DTOs.ClothingTypeDTOs;
using Clothy.CatalogService.BLL.DTOs.CollectionDTOs;
using Clothy.CatalogService.BLL.DTOs.MaterialDTOs;
using Clothy.CatalogService.BLL.DTOs.PhotoDTOs;
using Clothy.CatalogService.Domain.Entities.Clothe;
using Clothy.CatalogService.Domain.QueryParameters;
using Microsoft.AspNetCore.Http;
using Moq;

namespace Clothy.CatalogService.UnitTests.Helpers;

public static class ClotheItemFaker
    {
        public static ClotheItem GenerateClotheItem(Guid? id = null)
        {
            return new Faker<ClotheItem>()
                .RuleFor(c => c.Id, _ => id ?? Guid.NewGuid())
                .RuleFor(c => c.Name, f => f.Commerce.ProductName())
                .RuleFor(c => c.Slug, (f, c) => c.Name!.ToLower().Replace(" ", "-"))
                .RuleFor(c => c.Price, f => f.Finance.Amount(10, 500))
                .RuleFor(c => c.OldPrice, _ => null)
                .RuleFor(c => c.Description, f => f.Lorem.Sentence())
                .RuleFor(c => c.Gender, f => f.PickRandom<Gender>())
                .Generate();
        }
 
        public static ClotheSummaryDTO GenerateClotheSummaryDTO(Guid? id = null)
        {
            return new Faker<ClotheSummaryDTO>()
                .RuleFor(d => d.Id, _ => id ?? Guid.NewGuid())
                .RuleFor(d => d.Name, f => f.Commerce.ProductName())
                .RuleFor(d => d.Slug, (f, d) => d.Name!.ToLower().Replace(" ", "-"))
                .RuleFor(d => d.Price, f => f.Finance.Amount(10, 500))
                .RuleFor(d => d.IsAvailable, _ => true)
                .Generate();
        }
 
        public static List<ClotheSummaryDTO> GenerateClotheSummaryDTOs(int count = 5)
        {
            return Enumerable.Range(0, count)
                .Select(_ => GenerateClotheSummaryDTO())
                .ToList();
        }
 
        public static ClotheDetailDTO GenerateClotheDetailDTO(Guid? id = null, string? slug = null)
        {
            Faker faker = new Faker();
            string name = faker.Commerce.ProductName();
 
            return new ClotheDetailDTO
            {
                Id = id ?? Guid.NewGuid(),
                Name = name,
                Slug = slug ?? name.ToLower().Replace(" ", "-"),
                Price = faker.Finance.Amount(10, 500),
                Description = faker.Lorem.Sentence(),
                Gender = faker.PickRandom<Gender>(),
                Brand = new BrandReadDTO 
                { 
                    Id = Guid.NewGuid(), 
                    Name = faker.Company.CompanyName() 
                },
                ClothyType = new ClothingTypeReadDTO 
                { 
                    Id = Guid.NewGuid(), 
                    Name = faker.Commerce.Categories(1)[0] 
                },
                Collection = new CollectionReadDTO
                {
                    Id = Guid.NewGuid(), 
                    Name = faker.Commerce.Department()
                }
            };
        }
 
        public static ClotheCreateDTO GenerateCreateDTO(
            List<ClotheMaterialCreateDTO>? materials = null,
            List<ClothePhotoCreateDTO>? photos = null)
        {
            Faker faker = new Faker();
            string name = faker.Commerce.ProductName();
 
            List<ClotheMaterialCreateDTO> resolvedMaterials = materials ?? new List<ClotheMaterialCreateDTO>
            {
                new ClotheMaterialCreateDTO
                {
                    MaterialId = Guid.NewGuid(), Percentage = 100 
                }
            };
 
            Guid colorId = Guid.NewGuid();
            List<ClothePhotoCreateDTO> resolvedPhotos = photos ?? new List<ClothePhotoCreateDTO>
            {
                new ClothePhotoCreateDTO
                {
                    ColorId = colorId,
                    IsMain = true,
                    Photo = new Mock<IFormFile>().Object
                }
            };
 
            return new ClotheCreateDTO
            {
                Name = name,
                Slug = name.ToLower().Replace(" ", "-"),
                Price = faker.Finance.Amount(10, 500),
                Description = faker.Lorem.Sentence(),
                Gender = faker.PickRandom<Gender>(),
                BrandId = Guid.NewGuid(),
                ClothingTypeId = Guid.NewGuid(),
                CollectionId = Guid.NewGuid(),
                Materials = resolvedMaterials,
                AdditionalPhotos = resolvedPhotos,
                TagIds = new List<Guid> { Guid.NewGuid() }
            };
        }
 
        public static ClotheUpdateDTO GenerateUpdateDTO()
        {
            Faker faker = new Faker();
            string name = faker.Commerce.ProductName();
 
            return new ClotheUpdateDTO
            {
                Name = name,
                Slug = name.ToLower().Replace(" ", "-"),
                Price = faker.Finance.Amount(10, 500),
                Description = faker.Lorem.Sentence(),
                Gender = faker.PickRandom<Gender>(),
                BrandId = Guid.NewGuid(),
                ClothingTypeId = Guid.NewGuid(),
                CollectionId = Guid.NewGuid()
            };
        }
 
        public static ClotheItemSpecificationParameters GenerateParameters(int pageNumber = 1)
        {
            return new ClotheItemSpecificationParameters
            {
                PageNumber = pageNumber,
                PageSize = 10
            };
        }
    }