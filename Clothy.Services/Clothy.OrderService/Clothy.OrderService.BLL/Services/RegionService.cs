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

        public RegionService(IUnitOfWork unitOfWork, IMapper mapper, IEntityCacheService cacheService, IEntityCacheInvalidationService<Region> cacheInvalidationService)
        {
            this.unitOfWork = unitOfWork;
            this.mapper = mapper;
            this.cacheService = cacheService;
            this.cacheInvalidationService = cacheInvalidationService;
        }

        public async Task<PagedList<RegionReadDTO>> GetPagedAsync(RegionFilterDTO filter, CancellationToken cancellationToken = default)
        {
            bool usePageCache = filter.PageNumber <= MAX_CACHED_PAGES;
            string cacheKey = $"regions:page:{filter.PageNumber}:size:{filter.PageSize}";

            if (usePageCache)
            {
                PagedList<RegionReadDTO>? cached = await cacheService.GetOrSetAsync(
                    cacheKey,
                    async () =>
                    {
                        var (regions, totalCount) = await unitOfWork.Region.GetPagedAsync(filter, cancellationToken);
                        List<RegionReadDTO> dtos = mapper.Map<List<RegionReadDTO>>(regions);
                        return new PagedList<RegionReadDTO>(dtos, totalCount, filter.PageNumber, filter.PageSize);
                    },
                    memoryExpiration: MEMORY_TTL,
                    redisExpiration: REDIS_TTL
                );

                return cached;
            }
            else
            {
                var (regions, totalCount) = await unitOfWork.Region.GetPagedAsync(filter, cancellationToken);
                List<RegionReadDTO> dtos = mapper.Map<List<RegionReadDTO>>(regions);
                return new PagedList<RegionReadDTO>(dtos, totalCount, filter.PageNumber, filter.PageSize);
            }
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
            bool exists = await unitOfWork.Region.ExistByNameAndCityIdAsync(regionCreateDTO.Name, regionCreateDTO.CityId, cancellationToken: cancellationToken);
            if (exists) throw new AlreadyExistsException($"Region with name '{regionCreateDTO.Name}' already exists.");

            City? city = await unitOfWork.Cities.GetByIdAsync(regionCreateDTO.CityId, cancellationToken);
            if (city == null) throw new NotFoundException($"City not found with ID: {regionCreateDTO.CityId}");

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

            City? city = await unitOfWork.Cities.GetByIdAsync(regionUpdateDTO.CityId, cancellationToken);
            if (city == null) throw new NotFoundException($"City not found with ID: {regionUpdateDTO.CityId}");

            bool exists = await unitOfWork.Region.ExistByNameAndCityIdAsync(regionUpdateDTO.Name, regionUpdateDTO.CityId, id, cancellationToken);
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