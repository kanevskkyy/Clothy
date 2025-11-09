using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Clothy.OrderService.BLL.DTOs.SettlementDTOs;
using Clothy.OrderService.BLL.Interfaces;
using Clothy.OrderService.DAL.FilterDTOs;
using Clothy.OrderService.DAL.UOW;
using Clothy.OrderService.Domain.Entities;
using Clothy.Shared.Cache.Interfaces;
using Clothy.Shared.Helpers;
using Clothy.Shared.Helpers.Exceptions;

namespace Clothy.OrderService.BLL.Services
{
    public class SettlementService : ISettlementService
    {
        private IUnitOfWork unitOfWork;
        private IMapper mapper;
        private IEntityCacheService cacheService;
        private IEntityCacheInvalidationService<Settlement> cacheInvalidationService;

        private static TimeSpan MEMORY_TTL = TimeSpan.FromHours(1);
        private static TimeSpan REDIS_TTL = TimeSpan.FromDays(7);

        public SettlementService(IUnitOfWork unitOfWork, IMapper mapper, IEntityCacheService cacheService, IEntityCacheInvalidationService<Settlement> cacheInvalidationService)
        {
            this.unitOfWork = unitOfWork;
            this.mapper = mapper;
            this.cacheService = cacheService;
            this.cacheInvalidationService = cacheInvalidationService;
        }

        public async Task<SettlementReadDTO> CreateAsync(SettlementCreateDTO dto, CancellationToken cancellationToken = default)
        {
            bool nameAlreadyExists = await unitOfWork.Settlement.ExistsByNameAndRegionIdAsync(dto.Name, dto.RegionId, cancellationToken: cancellationToken);
            if (nameAlreadyExists) throw new AlreadyExistsException($"Settlement with name: {dto.Name} with this RegionId already exists");

            Region? region = await unitOfWork.Region.GetByIdAsync(dto.RegionId, cancellationToken);
            if (region == null) throw new NotFoundException($"Region with ID: {dto.RegionId}");

            Settlement settlement = mapper.Map<Settlement>(dto);
            settlement.Id = await unitOfWork.Settlement.AddAsync(settlement, cancellationToken);
            await unitOfWork.CommitAsync();

            await cacheInvalidationService.InvalidateAllAsync();

            return mapper.Map<SettlementReadDTO>(settlement);
        }

        public async Task<SettlementReadDTO> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            string cacheKey = $"settlement:{id}";

            SettlementReadDTO? cached = await cacheService.GetOrSetAsync(
                cacheKey,
                async () =>
                {
                    var settlement = await unitOfWork.Settlement.GetByIdAsync(id, cancellationToken);
                    if (settlement == null) throw new NotFoundException($"Settlement not found with ID: {id}");
                    return mapper.Map<SettlementReadDTO>(settlement);
                },
                memoryExpiration: MEMORY_TTL,
                redisExpiration: REDIS_TTL
            );

            return cached!;
        }

        public async Task<PagedList<SettlementReadDTO>> GetPagedAsync(SettlementFilterDTO filter, CancellationToken cancellationToken = default)
        {
            string cacheKey = $"settlements:page:{filter.PageNumber}:size:{filter.PageSize}";

            PagedList<SettlementReadDTO>? cached = await cacheService.GetOrSetAsync(
                cacheKey,
                async () =>
                {
                    var (items, totalCount) = await unitOfWork.Settlement.GetPagedAsync(filter, cancellationToken);
                    var dtos = mapper.Map<List<SettlementReadDTO>>(items);
                    return new PagedList<SettlementReadDTO>(dtos, totalCount, filter.PageNumber, filter.PageSize);
                },
                memoryExpiration: MEMORY_TTL,
                redisExpiration: REDIS_TTL
            );

            return cached!;
        }

        public async Task<SettlementReadDTO> UpdateAsync(Guid id, SettlementUpdateDTO dto, CancellationToken cancellationToken = default)
        {
            Settlement? settlement = await unitOfWork.Settlement.GetByIdAsync(id, cancellationToken);
            if (settlement == null) throw new NotFoundException($"Settlement not found with ID: {id}");

            bool nameAlreadyExists = await unitOfWork.Settlement.ExistsByNameAndRegionIdAsync(dto.Name, dto.RegionId, id, cancellationToken: cancellationToken);
            if (nameAlreadyExists) throw new AlreadyExistsException($"Settlement with name: {dto.Name} with this RegionId already exists");

            Region? region = await unitOfWork.Region.GetByIdAsync(dto.RegionId, cancellationToken);
            if (region == null) throw new NotFoundException($"Region not found with ID: {dto.RegionId}");

            mapper.Map(dto, settlement);
            await unitOfWork.Settlement.UpdateAsync(settlement);
            await unitOfWork.CommitAsync();

            await cacheInvalidationService.InvalidateByIdAsync(id);
            await cacheInvalidationService.InvalidateAllAsync();

            return mapper.Map<SettlementReadDTO>(settlement);
        }

        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            Settlement? settlement = await unitOfWork.Settlement.GetByIdAsync(id, cancellationToken);
            if (settlement == null) throw new NotFoundException($"Settlement not found with ID: {id}");

            await unitOfWork.Settlement.DeleteAsync(id, cancellationToken);
            await unitOfWork.CommitAsync();

            await cacheInvalidationService.InvalidateByIdAsync(id);
            await cacheInvalidationService.InvalidateAllAsync();
        }
    }
}