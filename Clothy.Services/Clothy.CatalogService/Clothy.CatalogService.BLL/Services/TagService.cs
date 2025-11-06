using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Clothy.Aggregator.Aggregate.RedisCache;
using Clothy.CatalogService.BLL.DTOs.TagDTOs;
using Clothy.CatalogService.BLL.Exceptions;
using Clothy.CatalogService.BLL.Interfaces;
using Clothy.CatalogService.DAL.UOW;
using Clothy.CatalogService.Domain.Entities;
using Clothy.Shared.Helpers.Exceptions;

namespace Clothy.CatalogService.BLL.Services
{
    public class TagService : ITagService
    {
        private IUnitOfWork unitOfWork;
        private IMapper mapper;
        private IFilterCacheInvalidationService filterCacheInvalidationService;

        public TagService(IUnitOfWork unitOfWork, IMapper mapper, IFilterCacheInvalidationService filterCacheInvalidationService)
        {
            this.filterCacheInvalidationService = filterCacheInvalidationService;
            this.unitOfWork = unitOfWork;
            this.mapper = mapper;
        }

        public async Task<TagReadDTO> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            Tag? tag = await unitOfWork.Tags.GetByIdAsync(id, cancellationToken);
            if (tag == null) throw new NotFoundException($"Tag not found with ID: {id}");
            
            return mapper.Map<TagReadDTO>(tag);
        }

        public async Task<List<TagReadDTO>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return mapper.Map<List<TagReadDTO>>(await unitOfWork.Tags.GetAllAsync(cancellationToken));
        }

        public async Task<List<TagWithCountDTO>> GetAllWithCountAsync(CancellationToken cancellationToken = default)
        {
            Dictionary<Tag, int> tagsWithCount = await unitOfWork.Tags.GetTagsWithStockCountAsync(cancellationToken);

            List<TagWithCountDTO> result = tagsWithCount
                .Select(pair => mapper.Map<TagWithCountDTO>(pair))
                .ToList();

            return result;
        }

        public async Task<TagReadDTO> CreateAsync(TagCreateDTO tagCreateDTO, CancellationToken cancellationToken = default)
        {
            bool exists = await unitOfWork.Tags.IsNameAlreadyExistsAsync(tagCreateDTO.Name, null, cancellationToken);

            if (exists) throw new AlreadyExistsException("Tag with this name already exists");

            Tag tag = mapper.Map<Tag>(tagCreateDTO);
            await unitOfWork.Tags.AddAsync(tag, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            await filterCacheInvalidationService.InvalidateAsync();

            return mapper.Map<TagReadDTO>(tag);
        }

        public async Task<TagReadDTO> UpdateAsync(Guid id, TagUpdateDTO tagUpdateDTO, CancellationToken cancellationToken = default)
        {
            Tag? tag = await unitOfWork.Tags.GetByIdAsync(id, cancellationToken);
            if (tag == null) throw new NotFoundException($"Tag not found with ID: {id}");

            bool exists = await unitOfWork.Tags.IsNameAlreadyExistsAsync(tagUpdateDTO.Name, id, cancellationToken);
            if (exists) throw new AlreadyExistsException("Tag with this name already exists");
            
            mapper.Map(tagUpdateDTO, tag);

            unitOfWork.Tags.Update(tag);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            await filterCacheInvalidationService.InvalidateAsync();

            return mapper.Map<TagReadDTO>(tag);
        }

        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            Tag? tag = await unitOfWork.Tags.GetByIdAsync(id, cancellationToken);
            if (tag == null) throw new NotFoundException($"Tag not found with ID: {id}");

            unitOfWork.Tags.Delete(tag);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            await filterCacheInvalidationService.InvalidateAsync();
        }
    }
}
