using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Clothy.OrderService.BLL.DTOs.CityDTOs;
using Clothy.OrderService.BLL.Interfaces;
using Clothy.OrderService.DAL.FilterDTOs;
using Clothy.OrderService.DAL.UOW;
using Clothy.OrderService.Domain.Entities;
using Clothy.Shared.Cache.Interfaces;
using Clothy.Shared.Helpers;
using Clothy.Shared.Helpers.Exceptions;

namespace Clothy.OrderService.BLL.Services
{
    public class CityService : ICityService
    {
        private IUnitOfWork unitOfWork;
        private IMapper mapper;
        private IEntityCacheService cacheService;
        private IEntityCacheInvalidationService<City> cacheInvalidationService;
        private static TimeSpan MEMORY_TTL = TimeSpan.FromMinutes(10);
        private static TimeSpan REDIS_TTL = TimeSpan.FromHours(1);
        private const int MAX_CACHED_PAGES = 3;

        public CityService(IUnitOfWork unitOfWork, IMapper mapper, IEntityCacheService cacheService, IEntityCacheInvalidationService<City> cacheInvalidationService)
        {
            this.unitOfWork = unitOfWork;
            this.mapper = mapper;
            this.cacheService = cacheService;
            this.cacheInvalidationService = cacheInvalidationService;
        }

        public async Task<PagedList<CityReadDTO>> GetPagedAsync(CityFilterDTO filter, CancellationToken cancellationToken = default)
        {
            bool usePageCache = filter.PageNumber <= MAX_CACHED_PAGES;
            string cacheKey = $"cities:page:{filter.PageNumber}:size:{filter.PageSize}";

            if (usePageCache)
            {
                PagedList<CityReadDTO>? cached = await cacheService.GetOrSetAsync(
                    cacheKey,
                    async () =>
                    {
                        var (cities, totalCount) = await unitOfWork.Cities.GetPagedAsync(filter, cancellationToken);
                        List<CityReadDTO> dtos = mapper.Map<List<CityReadDTO>>(cities);
                        return new PagedList<CityReadDTO>(dtos, totalCount, filter.PageNumber, filter.PageSize);
                    },
                    memoryExpiration: MEMORY_TTL,
                    redisExpiration: REDIS_TTL
                );

                return cached;
            }
            else
            {
                var (cities, totalCount) = await unitOfWork.Cities.GetPagedAsync(filter, cancellationToken);
                List<CityReadDTO> dtos = mapper.Map<List<CityReadDTO>>(cities);
                return new PagedList<CityReadDTO>(dtos, totalCount, filter.PageNumber, filter.PageSize);
            }
        }

        public async Task<CityReadDTO> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            string cacheKey = $"city:{id}";

            CityReadDTO? cached = await cacheService.GetOrSetAsync(
                cacheKey,
                async () =>
                {
                    City? city = await unitOfWork.Cities.GetByIdAsync(id, cancellationToken);
                    if (city == null) throw new NotFoundException($"City not found with ID: {id}");
                    return mapper.Map<CityReadDTO>(city);
                },
                memoryExpiration: MEMORY_TTL,
                redisExpiration: REDIS_TTL
            );

            return cached;
        }

        public async Task<CityReadDTO> CreateAsync(CityCreateDTO dto, CancellationToken cancellationToken = default)
        {
            bool exists = await unitOfWork.Cities.ExistsByNameAsync(dto.Name, null, cancellationToken);
            if (exists) throw new AlreadyExistsException($"City with name '{dto.Name}' already exists.");

            City city = mapper.Map<City>(dto);
            city.Id = await unitOfWork.Cities.AddAsync(city, cancellationToken);
            await unitOfWork.CommitAsync();

            await cacheInvalidationService.InvalidateAllAsync();

            return mapper.Map<CityReadDTO>(city);
        }

        public async Task<CityReadDTO> UpdateAsync(Guid id, CityUpdateDTO dto, CancellationToken cancellationToken = default)
        {
            City? city = await unitOfWork.Cities.GetByIdAsync(id, cancellationToken);
            if (city == null) throw new NotFoundException($"City not found with ID: {id}");

            bool exists = await unitOfWork.Cities.ExistsByNameAsync(dto.Name, id, cancellationToken);
            if (exists) throw new AlreadyExistsException($"City with name '{dto.Name}' already exists.");

            mapper.Map(dto, city);
            await unitOfWork.Cities.UpdateAsync(city, cancellationToken);
            await unitOfWork.CommitAsync();

            await cacheInvalidationService.InvalidateByIdAsync(city.Id);
            await cacheInvalidationService.InvalidateAllAsync();

            return mapper.Map<CityReadDTO>(city);
        }

        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            City? city = await unitOfWork.Cities.GetByIdAsync(id, cancellationToken);
            if (city == null) throw new NotFoundException($"City not found with ID: {id}");

            await unitOfWork.Cities.DeleteAsync(city.Id, cancellationToken);
            await unitOfWork.CommitAsync();

            await cacheInvalidationService.InvalidateByIdAsync(id);
            await cacheInvalidationService.InvalidateAllAsync();
        }
    }
}