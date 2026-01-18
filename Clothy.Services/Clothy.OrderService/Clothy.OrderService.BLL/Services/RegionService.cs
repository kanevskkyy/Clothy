using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Clothy.OrderService.BLL.DTOs.RegionDTOs;
using Clothy.OrderService.BLL.Interfaces;
using Clothy.OrderService.DAL.FilterDTOs;
using Clothy.OrderService.DAL.UOW;
using Clothy.OrderService.Domain.Entities;
using Clothy.Shared.Cache.Interfaces;
using Clothy.Shared.Helpers;
using Clothy.Shared.Helpers.Exceptions;

namespace Clothy.OrderService.BLL.Services
{
    public class RegionService : IRegionService
    {
        private IUnitOfWork unitOfWork;
        private IMapper mapper;
        private IEntityCacheService cacheService;
        private IEntityCacheInvalidationService<Region> cacheInvalidationService;
        private static TimeSpan MEMORY_TTL = TimeSpan.FromMinutes(10);
        private static TimeSpan REDIS_TTL = TimeSpan.FromHours(1);
        private const int MAX_CACHED_PAGES = 3;

        public RegionService(
            IUnitOfWork unitOfWork, 
            IMapper mapper, 
            IEntityCacheService cacheService, 
            IEntityCacheInvalidationService<Region> cacheInvalidationService)
        {
            this.unitOfWork = unitOfWork;
            this.mapper = mapper;
            this.cacheService = cacheService;
            this.cacheInvalidationService = cacheInvalidationService;
        }

        public async Task<List<RegionReadDTO>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            IEnumerable<Region> regions = await unitOfWork.Region.GetAllAsync(cancelletionToken: cancellationToken);
            List<RegionReadDTO> regionReadDTOs = mapper.Map<List<RegionReadDTO>>(regions);

            return regionReadDTOs;
        }

        public async Task<RegionReadDTO> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            string cacheKey = $"region:{id}";

            RegionReadDTO? cached = await cacheService.GetOrSetAsync(
                cacheKey,
                async () =>
                {
                    Region? region = await unitOfWork.Region.GetByIdAsync(id, cancellationToken);
                    if (region == null) throw new NotFoundException($"Region not found with ID: {id}");
                    return mapper.Map<RegionReadDTO>(region);
                },
                memoryExpiration: MEMORY_TTL,
                redisExpiration: REDIS_TTL
            );

            return cached;
        }

        public async Task<RegionReadDTO> CreateAsync(RegionCreateDTO regionCreateDTO, CancellationToken cancellationToken = default)
        {
            bool exists = await unitOfWork.Region.ExistByNameAsync(regionCreateDTO.Name, cancellationToken: cancellationToken);
            if (exists) throw new AlreadyExistsException($"Region with name '{regionCreateDTO.Name}' already exists.");

            Region region = mapper.Map<Region>(regionCreateDTO);
            region.Id = await unitOfWork.Region.AddAsync(region);
            await unitOfWork.CommitAsync();

            await cacheInvalidationService.InvalidateAllAsync();

            return mapper.Map<RegionReadDTO>(region);
        }

        public async Task<RegionReadDTO> UpdateAsync(Guid id, RegionUpdateDTO regionUpdateDTO, CancellationToken cancellationToken = default)
        {
            Region? region = await unitOfWork.Region.GetByIdAsync(id, cancellationToken);
            if (region == null) throw new NotFoundException($"Region not found with ID: {id}");

            bool exists = await unitOfWork.Region.ExistByNameAsync(regionUpdateDTO.Name, id, cancellationToken);
            if (exists) throw new AlreadyExistsException($"Region with name '{regionUpdateDTO.Name}' already exists.");

            mapper.Map(regionUpdateDTO, region);
            await unitOfWork.Region.UpdateAsync(region, cancellationToken);
            await unitOfWork.CommitAsync();

            await cacheInvalidationService.InvalidateByIdAsync(region.Id);
            await cacheInvalidationService.InvalidateAllAsync();

            return mapper.Map<RegionReadDTO>(region);
        }

        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            Region? region = await unitOfWork.Region.GetByIdAsync(id, cancellationToken);
            if (region == null) throw new NotFoundException($"Region not found with ID: {id}");

            await unitOfWork.Region.DeleteAsync(id, cancellationToken);
            await unitOfWork.CommitAsync();

            await cacheInvalidationService.InvalidateByIdAsync(id);
            await cacheInvalidationService.InvalidateAllAsync();
        }
    }
}