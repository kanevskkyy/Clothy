using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Clothy.OrderService.BLL.DTOs.PickupPointsDTOs;
using Clothy.OrderService.BLL.Interfaces;
using Clothy.OrderService.DAL.FilterDTOs;
using Clothy.OrderService.Domain.Entities;
using Clothy.Shared.Cache.Interfaces;
using Clothy.Shared.Helpers.Exceptions;
using Clothy.OrderService.DAL.UOW;
using Clothy.Shared.Helpers;

namespace Clothy.OrderService.BLL.Services
{
    public class PickupPointService : IPickupPointService
    {
        private IUnitOfWork unitOfWork;
        private IMapper mapper;
        private IEntityCacheService cacheService;
        private IEntityCacheInvalidationService<PickupPoints> cacheInvalidationService;

        private static TimeSpan MEMORY_TTL = TimeSpan.FromHours(6);
        private static TimeSpan REDIS_TTL = TimeSpan.FromDays(1);
        private const int MAX_CACHED_PAGES = 3;

        public PickupPointService(IUnitOfWork unitOfWork,
            IMapper mapper, 
            IEntityCacheService cacheService, 
            IEntityCacheInvalidationService<PickupPoints> cacheInvalidationService)
        {
            this.unitOfWork = unitOfWork;
            this.mapper = mapper;
            this.cacheService = cacheService;
            this.cacheInvalidationService = cacheInvalidationService;
        }

        public async Task<PickupPointReadDTO> CreateAsync(PickupPointCreateDTO pickupPointCreateDTO, CancellationToken cancellationToken = default)
        {
            Settlement? settlement = await unitOfWork.Settlement.GetByIdAsync(pickupPointCreateDTO.SettlementId, cancellationToken);
            if(settlement == null) throw new NotFoundException($"Settlement with ID: {pickupPointCreateDTO.SettlementId}");

            DeliveryProvider? deliveryProvider = await unitOfWork.DeliveryProviders.GetByIdAsync(pickupPointCreateDTO.DeliveryProviderId, cancellationToken);
            if (deliveryProvider == null) throw new NotFoundException($"DeliveryProvider with ID: {pickupPointCreateDTO.DeliveryProviderId}");

            PickupPoints entity = mapper.Map<PickupPoints>(pickupPointCreateDTO);
            entity.Id = await unitOfWork.PickupPoint.AddAsync(entity, cancellationToken);
            await unitOfWork.CommitAsync();

            await cacheInvalidationService.InvalidateAllAsync();

            return mapper.Map<PickupPointReadDTO>(entity);
        }

        public async Task<PickupPointReadDTO> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            string cacheKey = $"pickup-point:{id}";

            PickupPointReadDTO? cached = await cacheService.GetOrSetAsync(
                cacheKey,
                async () =>
                {
                    PickupPoints? entity = await unitOfWork.PickupPoint.GetByIdAsync(id, cancellationToken);
                    if (entity == null) throw new NotFoundException($"PickupPoint not found with ID: {id}");
                    return mapper.Map<PickupPointReadDTO>(entity);
                },
                memoryExpiration: MEMORY_TTL,
                redisExpiration: REDIS_TTL
            );

            return cached;
        }

        public async Task<PagedList<PickupPointReadDTO>?> GetPagedAsync(PickupPointFilterDTO filter, CancellationToken cancellationToken = default)
        {
            bool usePageCache = filter.PageNumber <= MAX_CACHED_PAGES;

            if (usePageCache)
            {
                return await cacheService.GetOrSetAsync(
                    filter.ToCacheKey(),
                    async () => await FetchPickupPointsAsync(filter, cancellationToken),
                    memoryExpiration: MEMORY_TTL,
                    redisExpiration: REDIS_TTL
                );
            }

            return await FetchPickupPointsAsync(filter, cancellationToken);
        }

        private async Task<PagedList<PickupPointReadDTO>> FetchPickupPointsAsync(PickupPointFilterDTO filter, CancellationToken cancellationToken)
        {
            var (items, totalCount) = await unitOfWork.PickupPoint.GetPagedAsync(filter, cancellationToken);
            List<PickupPointReadDTO> dtos = mapper.Map<List<PickupPointReadDTO>>(items);
            
            return new PagedList<PickupPointReadDTO>(dtos, totalCount, filter.PageNumber, filter.PageSize);
        }

        public async Task<PickupPointReadDTO> UpdateAsync(Guid id, PickupPointUpdateDTO pickupPointUpdateDTO, CancellationToken cancellationToken = default)
        {
            PickupPoints? entity = await unitOfWork.PickupPoint.GetByIdAsync(id, cancellationToken);
            if (entity == null) throw new NotFoundException($"PickupPoint not found with ID: {id}");

            Settlement? settlement = await unitOfWork.Settlement.GetByIdAsync(pickupPointUpdateDTO.SettlementId, cancellationToken);
            if (settlement == null) throw new NotFoundException($"Settlement with ID: {pickupPointUpdateDTO.SettlementId}");

            DeliveryProvider? provider = await unitOfWork.DeliveryProviders.GetByIdAsync(pickupPointUpdateDTO.DeliveryProviderId, cancellationToken);
            if (provider == null) throw new NotFoundException($"DeliveryProvider not found with ID: {pickupPointUpdateDTO.DeliveryProviderId}");

            mapper.Map(pickupPointUpdateDTO, entity);
            await unitOfWork.PickupPoint.UpdateAsync(entity);
            await unitOfWork.CommitAsync();

            await cacheInvalidationService.InvalidateByIdAsync(id);
            await cacheInvalidationService.InvalidateAllAsync();

            return mapper.Map<PickupPointReadDTO>(entity);
        }

        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            PickupPoints? entity = await unitOfWork.PickupPoint.GetByIdAsync(id, cancellationToken);
            if (entity == null) throw new NotFoundException($"PickupPoint not found with ID: {id}");

            await unitOfWork.PickupPoint.DeleteAsync(id, cancellationToken);
            await unitOfWork.CommitAsync();

            await cacheInvalidationService.InvalidateByIdAsync(id);
            await cacheInvalidationService.InvalidateAllAsync();
        }
    }
}