using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Clothy.CatalogService.BLL.DTOs.ClothingTypeDTOs;
using Clothy.CatalogService.BLL.DTOs.CollectionDTOs;
using Clothy.CatalogService.BLL.Exceptions;
using Clothy.CatalogService.BLL.Interfaces;
using Clothy.CatalogService.DAL.UOW;
using Clothy.CatalogService.Domain.Entities;
using Clothy.Shared.Exceptions;

namespace Clothy.CatalogService.BLL.Services
{
    public class CollectionService : ICollectionService
    {
        private IUnitOfWork unitOfWork;
        private IMapper mapper;

        public CollectionService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            this.unitOfWork = unitOfWork;
            this.mapper = mapper;
        }

        public async Task<CollectionReadDTO> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            Collection? collection = await unitOfWork.Collections.GetByIdAsync(id, cancellationToken);
            if (collection == null) throw new NotFoundException($"Collection not found with ID: {id}");

            return mapper.Map<CollectionReadDTO>(collection);
        }

        public async Task<List<CollectionReadDTO>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return mapper.Map<List<CollectionReadDTO>>(await unitOfWork.Collections.GetAllAsync(cancellationToken));
        }

        public async Task<List<CollectionWithCountDTO>> GetAllWithCountAsync(CancellationToken cancellationToken = default)
        {
            Dictionary<Collection, int> collectionsWithCount = await unitOfWork.Collections.GetCollectionsCountWithStockAsync(cancellationToken);
            return collectionsWithCount
                .Select(pair => mapper.Map<CollectionWithCountDTO>(pair))
                .ToList();
        }

        public async Task<CollectionReadDTO> CreateAsync(CollectionCreateDTO dto, CancellationToken cancellationToken = default)
        {
            bool exists = await unitOfWork.Collections.IsNameAlreadyExistsAsync(dto.Name, null, cancellationToken);
            if (exists) throw new AlreadyExistsException("Collection with this name already exists");

            exists = await unitOfWork.Collections.IsSlugAlreadyExistsAsync(dto.Slug, null, cancellationToken);
            if (exists) throw new AlreadyExistsException("Collection with this slug already exists");

            Collection collection = mapper.Map<Collection>(dto);
            await unitOfWork.Collections.AddAsync(collection, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return mapper.Map<CollectionReadDTO>(collection);
        }

        public async Task<CollectionReadDTO> UpdateAsync(Guid id, CollectionUpdateDTO dto, CancellationToken cancellationToken = default)
        {
            Collection? collection = await unitOfWork.Collections.GetByIdAsync(id, cancellationToken);
            if (collection == null) throw new NotFoundException($"Collection not found with ID: {id}");

            bool exists = await unitOfWork.Collections.IsNameAlreadyExistsAsync(dto.Name, id, cancellationToken);
            if (exists) throw new AlreadyExistsException("Collection with this name already exists");

            exists = await unitOfWork.Collections.IsSlugAlreadyExistsAsync(dto.Slug, id, cancellationToken);
            if (exists) throw new AlreadyExistsException("Collection with this slug already exists");

            mapper.Map(dto, collection);

            unitOfWork.Collections.Update(collection);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return mapper.Map<CollectionReadDTO>(collection);
        }

        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            Collection? collection = await unitOfWork.Collections.GetByIdAsync(id, cancellationToken);
            if (collection == null) throw new NotFoundException($"Collection not found with ID: {id}");

            unitOfWork.Collections.Delete(collection);
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
