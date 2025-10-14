using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Clothy.CatalogService.BLL.DTOs.ClothingTypeDTOs;
using Clothy.CatalogService.BLL.Exceptions;
using Clothy.CatalogService.BLL.Interfaces;
using Clothy.CatalogService.DAL.UOW;
using Clothy.CatalogService.Domain.Entities;

namespace Clothy.CatalogService.BLL.Services
{
    public class ClothingTypeService : IClothingTypeService
    {
        private IUnitOfWork unitOfWork;
        private IMapper mapper;

        public ClothingTypeService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            this.unitOfWork = unitOfWork;
            this.mapper = mapper;
        }

        public async Task<ClothingTypeReadDTO> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            ClothingType? clothingType = await unitOfWork.ClothingTypes.GetByIdAsync(id, cancellationToken);
            if (clothingType == null) throw new NotFoundException($"ClothingType not found with ID: {id}");
            
            return mapper.Map<ClothingTypeReadDTO>(clothingType);
        }

        public async Task<List<ClothingTypeReadDTO>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return mapper.Map<List<ClothingTypeReadDTO>>(await unitOfWork.ClothingTypes.GetAllAsync(cancellationToken));
        }

        public async Task<ClothingTypeReadDTO> CreateAsync(ClothingTypeCreateDTO clothingTypeCreateDTO, CancellationToken cancellationToken = default)
        {
            bool exists = await unitOfWork.ClothingTypes.IsNameAlreadyExistsAsync(clothingTypeCreateDTO.Name, null, cancellationToken);
            if (exists) throw new AlreadyExistsException("ClothingType with this name already exists");

            exists = await unitOfWork.ClothingTypes.IsSlugAlreadyExistsAsync(clothingTypeCreateDTO.Slug, null, cancellationToken);
            if (exists) throw new AlreadyExistsException("ClothingType with this slug already exists");

            ClothingType clothingType = mapper.Map<ClothingType>(clothingTypeCreateDTO);
            await unitOfWork.ClothingTypes.AddAsync(clothingType, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return mapper.Map<ClothingTypeReadDTO>(clothingType);
        }

        public async Task<ClothingTypeReadDTO> UpdateAsync(Guid id, ClothingTypeUpdateDTO clothingTypeUpdateDTO, CancellationToken cancellationToken = default)
        {
            ClothingType? clothingType = await unitOfWork.ClothingTypes.GetByIdAsync(id, cancellationToken);
            if (clothingType == null) throw new NotFoundException($"ClothingType not found with ID: {id}");

            bool exists = await unitOfWork.ClothingTypes.IsNameAlreadyExistsAsync(clothingTypeUpdateDTO.Name, id, cancellationToken);
            if (exists) throw new AlreadyExistsException("ClothingType with this name already exists");

            exists = await unitOfWork.ClothingTypes.IsSlugAlreadyExistsAsync(clothingTypeUpdateDTO.Slug, id, cancellationToken);
            if (exists) throw new AlreadyExistsException("ClothingType with this slug already exists");

            mapper.Map(clothingTypeUpdateDTO, clothingType);

            unitOfWork.ClothingTypes.Update(clothingType);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return mapper.Map<ClothingTypeReadDTO>(clothingType);
        }

        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            ClothingType? clothingType = await unitOfWork.ClothingTypes.GetByIdAsync(id, cancellationToken);
            if (clothingType == null) throw new NotFoundException($"ClothingType not found with ID: {id}");

            unitOfWork.ClothingTypes.Delete(clothingType);
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
