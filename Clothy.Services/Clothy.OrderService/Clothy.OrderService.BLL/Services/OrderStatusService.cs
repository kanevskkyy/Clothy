using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Clothy.OrderService.BLL.DTOs.OrderStatusDTOs;
using Clothy.OrderService.BLL.Interfaces;
using Clothy.OrderService.DAL.UOW;
using Clothy.OrderService.Domain.Entities;
using Clothy.Shared.Cache.Interfaces;
using Clothy.Shared.Helpers.CloudinaryConfig;
using Clothy.Shared.Helpers.Exceptions;

namespace Clothy.OrderService.BLL.Services
{
    public class OrderStatusService : IOrderStatusService
    {
        private IUnitOfWork unitOfWork;
        private IMapper mapper;
        private IImageService imageService;
        private IEntityCacheService cacheService;
        private IEntityCacheInvalidationService<OrderStatus> cacheInvalidationService;
        private const string ALL_STATUSES_KEY = "order-status:all";
        private static readonly TimeSpan MEMORY_TTL_ORDER_STATUS = TimeSpan.FromHours(1);
        private static readonly TimeSpan REDIS_TTL_ORDER_STATUS = TimeSpan.FromDays(7);

        public OrderStatusService(IUnitOfWork unitOfWork, IMapper mapper, IImageService imageService, IEntityCacheService cacheService, IEntityCacheInvalidationService<OrderStatus> cacheInvalidationService)
        {
            this.unitOfWork = unitOfWork;
            this.mapper = mapper;
            this.imageService = imageService;
            this.cacheService = cacheService;
            this.cacheInvalidationService = cacheInvalidationService;
        }

        public async Task<List<OrderStatusReadDTO>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var cached = await cacheService.GetOrSetAsync(
                ALL_STATUSES_KEY,
                async () =>
                {
                    IEnumerable<OrderStatus> statuses = await unitOfWork.OrderStatuses.GetAllAsync(cancellationToken);
                    return mapper.Map<List<OrderStatusReadDTO>>(statuses.ToList());
                },
                memoryExpiration: MEMORY_TTL_ORDER_STATUS,
                redisExpiration: REDIS_TTL_ORDER_STATUS
            );

            return cached!;
        }

        public async Task<OrderStatusReadDTO> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            string cacheKey = $"order-status:{id}";

            var cached = await cacheService.GetOrSetAsync(
                cacheKey,
                async () =>
                {
                    OrderStatus? status = await unitOfWork.OrderStatuses.GetByIdAsync(id, cancellationToken);
                    if (status == null) throw new NotFoundException($"OrderStatus not found with ID: {id}");
                    return mapper.Map<OrderStatusReadDTO>(status);
                },
                memoryExpiration: MEMORY_TTL_ORDER_STATUS,
                redisExpiration: REDIS_TTL_ORDER_STATUS
            );

            return cached!;
        }

        public async Task<OrderStatusReadDTO> CreateAsync(OrderStatusCreateDTO dto, CancellationToken cancellationToken = default)
        {
            bool exists = await unitOfWork.OrderStatuses.ExistsByNameAsync(dto.Name, null, cancellationToken);
            if (exists) throw new AlreadyExistsException($"OrderStatus with name '{dto.Name}' already exists.");

            OrderStatus status = mapper.Map<OrderStatus>(dto);
            status.IconUrl = await imageService.UploadAsync(dto.Icon, "order-statuses");
            status.Id = await unitOfWork.OrderStatuses.AddAsync(status, cancellationToken);
            await unitOfWork.CommitAsync();

            await cacheInvalidationService.InvalidateAllAsync();

            return mapper.Map<OrderStatusReadDTO>(status);
        }

        public async Task<OrderStatusReadDTO> UpdateAsync(Guid id, OrderStatusUpdateDTO dto, CancellationToken cancellationToken = default)
        {
            OrderStatus? status = await unitOfWork.OrderStatuses.GetByIdAsync(id, cancellationToken);
            if (status == null) throw new NotFoundException($"OrderStatus not found with ID: {id}");

            bool exists = await unitOfWork.OrderStatuses.ExistsByNameAsync(dto.Name, id, cancellationToken);
            if (exists) throw new AlreadyExistsException($"OrderStatus with name '{dto.Name}' already exists.");

            if (dto.Icon != null)
            {
                if (!string.IsNullOrEmpty(status.IconUrl)) await imageService.DeleteImageAsync(status.IconUrl);
                status.IconUrl = await imageService.UploadAsync(dto.Icon, "order-statuses");
            }

            mapper.Map(dto, status);
            await unitOfWork.OrderStatuses.UpdateAsync(status, cancellationToken);
            await unitOfWork.CommitAsync();

            await cacheInvalidationService.InvalidateByIdAsync(id);
            await cacheInvalidationService.InvalidateAllAsync();

            return mapper.Map<OrderStatusReadDTO>(status);
        }

        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            OrderStatus? status = await unitOfWork.OrderStatuses.GetByIdAsync(id, cancellationToken);
            if (status == null) throw new NotFoundException($"OrderStatus not found with ID: {id}");

            if (!string.IsNullOrEmpty(status.IconUrl)) await imageService.DeleteImageAsync(status.IconUrl);

            await unitOfWork.OrderStatuses.DeleteAsync(id, cancellationToken);
            await unitOfWork.CommitAsync();

            await cacheInvalidationService.InvalidateByIdAsync(id);
            await cacheInvalidationService.InvalidateAllAsync();
        }
    }
}