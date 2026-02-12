using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Clothy.OrderService.BLL.DTOs.DeliveryProviderDTOs;
using Clothy.OrderService.BLL.Interfaces;
using Clothy.OrderService.DAL.UOW;
using Clothy.OrderService.Domain.Entities;
using Clothy.Shared.Cache.Interfaces;
using Clothy.Shared.Helpers.CloudinaryConfig.ImageService;
using Clothy.Shared.Helpers.Exceptions;

namespace Clothy.OrderService.BLL.Services
{
    public class DeliveryProviderService : IDeliveryProviderService
    {
        private IUnitOfWork unitOfWork;
        private IMapper mapper;
        private IImageService imageService;
        private IEntityCacheService cacheService;
        private IEntityCacheInvalidationService<DeliveryProvider> cacheInvalidationService;
        private IEntityCacheInvalidationService<PickupPoints> pickupPointInvalidationService;
        
        private const string ALL_CACHE_KEY = "delivery-provider:all";
        private static TimeSpan MEMORY_TTL_DELIVERY_PROVIDER = TimeSpan.FromHours(12);
        private static TimeSpan REDIS_TTL_DELIVERY_PROVIDER = TimeSpan.FromDays(7);

        public DeliveryProviderService(IUnitOfWork unitOfWork, 
            IMapper mapper,
            IImageService imageService,
            IEntityCacheService cacheService,
            IEntityCacheInvalidationService<DeliveryProvider> cacheInvalidationService,
            IEntityCacheInvalidationService<PickupPoints> pickupPointInvalidationService)
        {
            this.unitOfWork = unitOfWork;
            this.mapper = mapper;
            this.imageService = imageService;
            this.cacheService = cacheService;
            this.cacheInvalidationService = cacheInvalidationService;
            this.pickupPointInvalidationService = pickupPointInvalidationService;
        }

        public async Task<List<DeliveryProviderReadDTO>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            List<DeliveryProviderReadDTO>? cached = await cacheService.GetOrSetAsync(
                ALL_CACHE_KEY,
                async () =>
                {
                    IEnumerable<DeliveryProvider> providers = await unitOfWork.DeliveryProviders.GetAllAsync(cancellationToken);
                    return mapper.Map<List<DeliveryProviderReadDTO>>(providers.ToList());
                },
                memoryExpiration: MEMORY_TTL_DELIVERY_PROVIDER,
                redisExpiration: REDIS_TTL_DELIVERY_PROVIDER
            );

            return cached!;
        }

        public async Task<DeliveryProviderReadDTO> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            string cacheKey = $"delivery-provider:{id}";

            DeliveryProviderReadDTO? cached = await cacheService.GetOrSetAsync(
                cacheKey,
                async () =>
                {
                    DeliveryProvider? provider = await unitOfWork.DeliveryProviders.GetByIdAsync(id, cancellationToken);
                    if (provider == null) throw new NotFoundException($"DeliveryProvider not found with ID: {id}");
                    return mapper.Map<DeliveryProviderReadDTO>(provider);
                },
                memoryExpiration: MEMORY_TTL_DELIVERY_PROVIDER,
                redisExpiration: REDIS_TTL_DELIVERY_PROVIDER
            );

            return cached!;
        }

        public async Task<DeliveryProviderReadDTO> CreateAsync(DeliveryProviderCreateDTO deliveryProviderCreateDTO, CancellationToken cancellationToken = default)
        {
            bool exists = await unitOfWork.DeliveryProviders.ExistsByNameAsync(deliveryProviderCreateDTO.Name, null, cancellationToken);
            if (exists) throw new AlreadyExistsException($"DeliveryProvider with name '{deliveryProviderCreateDTO.Name}' already exists.");

            DeliveryProvider provider = mapper.Map<DeliveryProvider>(deliveryProviderCreateDTO);
            provider.IconUrl = await imageService.UploadAsync(deliveryProviderCreateDTO.Icon, "delivery-providers", true);

            provider.Id = await unitOfWork.DeliveryProviders.AddAsync(provider, cancellationToken);
            await unitOfWork.CommitAsync();

            await cacheInvalidationService.InvalidateAllAsync();

            return mapper.Map<DeliveryProviderReadDTO>(provider);
        }

        public async Task<DeliveryProviderReadDTO> UpdateAsync(Guid id, DeliveryProviderUpdateDTO deliveryProviderUpdateDTO, CancellationToken cancellationToken = default)
        {
            DeliveryProvider? provider = await unitOfWork.DeliveryProviders.GetByIdAsync(id, cancellationToken);
            if (provider == null) throw new NotFoundException($"DeliveryProvider not found with ID: {id}");

            bool exists = await unitOfWork.DeliveryProviders.ExistsByNameAsync(deliveryProviderUpdateDTO.Name, id, cancellationToken);
            if (exists) throw new AlreadyExistsException($"DeliveryProvider with name '{deliveryProviderUpdateDTO.Name}' already exists.");

            if (deliveryProviderUpdateDTO.Icon != null)
            {
                if (!string.IsNullOrEmpty(provider.IconUrl)) await imageService.DeleteImageAsync(provider.IconUrl);
                provider.IconUrl = await imageService.UploadAsync(deliveryProviderUpdateDTO.Icon, "delivery-providers");
            }

            mapper.Map(deliveryProviderUpdateDTO, provider);
            await unitOfWork.DeliveryProviders.UpdateAsync(provider, cancellationToken);
            await unitOfWork.CommitAsync();

            await cacheInvalidationService.InvalidateByIdAsync(provider.Id);
            await cacheInvalidationService.InvalidateAllAsync();

            return mapper.Map<DeliveryProviderReadDTO>(provider);
        }

        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            DeliveryProvider? provider = await unitOfWork.DeliveryProviders.GetByIdAsync(id, cancellationToken);
            if (provider == null) throw new NotFoundException($"DeliveryProvider not found with ID: {id}");

            if (!string.IsNullOrEmpty(provider.IconUrl)) await imageService.DeleteImageAsync(provider.IconUrl);

            await unitOfWork.DeliveryProviders.DeleteAsync(id, cancellationToken);
            await unitOfWork.CommitAsync();

            await cacheInvalidationService.InvalidateByIdAsync(id);
            await cacheInvalidationService.InvalidateAllAsync();
            await pickupPointInvalidationService.InvalidateAllAsync();
        }
    }
}