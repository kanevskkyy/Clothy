using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Clothy.CatalogService.BLL.DTOs.BrandDTOs;
using Clothy.CatalogService.BLL.Exceptions;
using Clothy.CatalogService.BLL.Helpers;
using Clothy.CatalogService.BLL.Interfaces;
using Clothy.CatalogService.DAL.UOW;
using Clothy.CatalogService.Domain.Entities;
using Clothy.Shared.Helpers.CloudinaryConfig;
using Clothy.Shared.Helpers.Exceptions;

namespace Clothy.CatalogService.BLL.Services
{
    public class BrandService : IBrandService
    {
        private IUnitOfWork unitOfWork;
        private IMapper mapper;
        private IImageService imageService;

        public BrandService(IUnitOfWork unitOfWork, IMapper mapper, IImageService imageService)
        {
            this.unitOfWork = unitOfWork;
            this.mapper = mapper;
            this.imageService = imageService;
        }

        public async Task<List<BrandReadDTO>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return mapper.Map<List<BrandReadDTO>>(await unitOfWork.Brands.GetAllAsync(cancellationToken));
        }

        public async Task<BrandReadDTO> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            Brand? brand = await unitOfWork.Brands.GetByIdAsync(id, cancellationToken);
            if (brand == null) throw new NotFoundException($"Brand not found with ID: {id}");

            return mapper.Map<BrandReadDTO>(brand);
        }

        public async Task<BrandReadDTO> CreateAsync(BrandCreateDTO dto, CancellationToken cancellationToken = default)
        {
            bool exists = await unitOfWork.Brands.IsNameAlreadyExistsAsync(dto.Name, null, cancellationToken);
            if (exists) throw new AlreadyExistsException("Brand with this name already exists");

            exists = await unitOfWork.Brands.IsSlugAlreadyExistsAsync(dto.Slug, null, cancellationToken);
            if (exists) throw new AlreadyExistsException("Brand with this slug already exists");

            string photoUrl = await imageService.UploadAsync(dto.Photo, "brands");

            Brand brand = mapper.Map<Brand>(dto);
            brand.PhotoURL = photoUrl;

            await unitOfWork.Brands.AddAsync(brand, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return mapper.Map<BrandReadDTO>(brand);
        }

        public async Task<BrandReadDTO> UpdateAsync(Guid id, BrandUpdateDTO dto, CancellationToken cancellationToken = default)
        {
            Brand? brand = await unitOfWork.Brands.GetByIdAsync(id, cancellationToken);
            if (brand == null) throw new NotFoundException($"Brand not found with ID: {id}");

            bool exists = await unitOfWork.Brands.IsNameAlreadyExistsAsync(dto.Name, id, cancellationToken);
            if (exists) throw new AlreadyExistsException("Brand with this name already exists");

            exists = await unitOfWork.Brands.IsSlugAlreadyExistsAsync(dto.Slug, id, cancellationToken);
            if (exists) throw new AlreadyExistsException("Brand with this slug already exists");

            if (dto.Photo != null)
            {
                if (!string.IsNullOrEmpty(brand.PhotoURL)) await imageService.DeleteImageAsync(brand.PhotoURL);
                brand.PhotoURL = await imageService.UploadAsync(dto.Photo, "brands");
            }

            mapper.Map(dto, brand);
            unitOfWork.Brands.Update(brand);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return mapper.Map<BrandReadDTO>(brand);
        }

        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            Brand? brand = await unitOfWork.Brands.GetByIdAsync(id, cancellationToken);
            if (brand == null) throw new NotFoundException($"Brand not found with ID: {id}");

            if (!string.IsNullOrEmpty(brand.PhotoURL)) await imageService.DeleteImageAsync(brand.PhotoURL);

            unitOfWork.Brands.Delete(brand);
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
