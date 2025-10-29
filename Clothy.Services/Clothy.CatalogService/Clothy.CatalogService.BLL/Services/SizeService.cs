using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Clothy.CatalogService.BLL.DTOs.SizeDTOs;
using Clothy.CatalogService.BLL.Exceptions;
using Clothy.CatalogService.BLL.Interfaces;
using Clothy.CatalogService.DAL.UOW;
using Clothy.CatalogService.Domain.Entities;

namespace Clothy.CatalogService.BLL.Services
{
    public class SizeService : ISizeService
    {
        private IUnitOfWork unitOfWork;
        private IMapper mapper;

        public SizeService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            this.unitOfWork = unitOfWork;
            this.mapper = mapper;
        }

        public async Task<SizeReadDTO> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            Size? size = await unitOfWork.Sizes.GetByIdAsync(id, cancellationToken);
            if (size == null) throw new NotFoundException($"Size not found with ID: {id}");
            
            return mapper.Map<SizeReadDTO>(size);
        }

        public async Task<List<SizeReadDTO>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return mapper.Map<List<SizeReadDTO>>(await unitOfWork.Sizes.GetAllAsync(cancellationToken));
        }

        public async Task<SizeReadDTO> CreateAsync(SizeCreateDTO sizeCreateDTO, CancellationToken cancellationToken = default)
        {
            bool exists = await unitOfWork.Sizes.IsNameAlreadyExistsAsync(sizeCreateDTO.Name, null, cancellationToken);
            if (exists) throw new AlreadyExistsException("Size with this name already exists");

            Size size = mapper.Map<Size>(sizeCreateDTO);
            await unitOfWork.Sizes.AddAsync(size, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return mapper.Map<SizeReadDTO>(size);
        }

        public async Task<SizeReadDTO> UpdateAsync(Guid id, SizeUpdateDTO sizeUpdateDTO, CancellationToken cancellationToken = default)
        {
            Size? size = await unitOfWork.Sizes.GetByIdAsync(id, cancellationToken);
            if (size == null) throw new NotFoundException($"Size not found with ID: {id}");

            bool exists = await unitOfWork.Sizes.IsNameAlreadyExistsAsync(sizeUpdateDTO.Name, id, cancellationToken);
            if (exists) throw new AlreadyExistsException("Size with this name already exists");

            mapper.Map(sizeUpdateDTO, size);

            unitOfWork.Sizes.Update(size);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return mapper.Map<SizeReadDTO>(size);
        }

        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            Size? size = await unitOfWork.Sizes.GetByIdAsync(id, cancellationToken);
            if (size == null) throw new NotFoundException($"Size not found with ID: {id}");

            unitOfWork.Sizes.Delete(size);
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}