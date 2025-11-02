using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Clothy.CatalogService.BLL.DTOs.MaterialDTOs;
using Clothy.CatalogService.BLL.Exceptions;
using Clothy.CatalogService.BLL.Interfaces;
using Clothy.CatalogService.DAL.UOW;
using Clothy.CatalogService.Domain.Entities;
using Clothy.Shared.Exceptions;

namespace Clothy.CatalogService.BLL.Services
{
    public class MaterialService : IMaterialService
    {
        private IUnitOfWork unitOfWork;
        private IMapper mapper;

        public MaterialService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            this.unitOfWork = unitOfWork;
            this.mapper = mapper;
        }

        public async Task<MaterialReadDTO> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            Material? material = await unitOfWork.Materials.GetByIdAsync(id, cancellationToken);
            if (material == null) throw new NotFoundException($"Material not found with ID: {id}");
            return mapper.Map<MaterialReadDTO>(material);
        }

        public async Task<List<MaterialReadDTO>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return mapper.Map<List<MaterialReadDTO>>(await unitOfWork.Materials.GetAllAsync(cancellationToken));
        }

        public async Task<List<MaterialWithCountDTO>> GetAllWithCountAsync(CancellationToken cancellationToken = default)
        {
            return (await unitOfWork.Materials.GetMaterialsWithStockAsync(cancellationToken))
                .Select(pair => mapper.Map<MaterialWithCountDTO>(pair))
                .ToList();
        }

        public async Task<MaterialReadDTO> CreateAsync(MaterialCreateDTO materialCreateDTO, CancellationToken cancellationToken = default)
        {
            bool exists = await unitOfWork.Materials.IsNameAlreadyExistsAsync(materialCreateDTO.Name, null, cancellationToken);
            if (exists) throw new AlreadyExistsException("Material with this name already exists");

            exists = await unitOfWork.Materials.IsSlugAlreadyExistsAsync(materialCreateDTO.Slug, null, cancellationToken);
            if (exists) throw new AlreadyExistsException("Material with this slug already exists");

            Material material = mapper.Map<Material>(materialCreateDTO);
            await unitOfWork.Materials.AddAsync(material, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return mapper.Map<MaterialReadDTO>(material);
        }

        public async Task<MaterialReadDTO> UpdateAsync(Guid id, MaterialUpdateDTO materialUpdateDTO, CancellationToken cancellationToken = default)
        {
            Material? material = await unitOfWork.Materials.GetByIdAsync(id, cancellationToken);
            if (material == null) throw new NotFoundException($"Material not found with ID: {id}");

            bool exists = await unitOfWork.Materials.IsNameAlreadyExistsAsync(materialUpdateDTO.Name, id, cancellationToken);
            if (exists) throw new AlreadyExistsException("Material with this name already exists");

            exists = await unitOfWork.Materials.IsSlugAlreadyExistsAsync(materialUpdateDTO.Slug, id, cancellationToken);
            if (exists) throw new AlreadyExistsException("Material with this slug already exists");

            mapper.Map(materialUpdateDTO, material);

            unitOfWork.Materials.Update(material);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return mapper.Map<MaterialReadDTO>(material);
        }

        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            Material? material = await unitOfWork.Materials.GetByIdAsync(id, cancellationToken);
            if (material == null) throw new NotFoundException($"Material not found with ID: {id}");

            unitOfWork.Materials.Delete(material);
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
