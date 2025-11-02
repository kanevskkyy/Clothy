using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Clothy.OrderService.BLL.DTOs.DeliveryProviderDTOs;
using Clothy.OrderService.BLL.Interfaces;
using Clothy.OrderService.DAL.UOW;
using Clothy.OrderService.Domain.Entities;
using Clothy.Shared.Exceptions;
using Clothy.Shared.Helpers.CloudinaryConfig;

namespace Clothy.OrderService.BLL.Services
{
    public class DeliveryProviderService : IDeliveryProviderService
    {
        private IUnitOfWork unitOfWork;
        private IMapper mapper;
        private IImageService imageService;

        public DeliveryProviderService(IUnitOfWork unitOfWork, IMapper mapper, IImageService imageService)
        {
            this.unitOfWork = unitOfWork;
            this.mapper = mapper;
            this.imageService = imageService;
        }

        public async Task<List<DeliveryProviderReadDTO>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            IEnumerable<DeliveryProvider> providers = await unitOfWork.DeliveryProviders.GetAllAsync(cancellationToken);
           
            return mapper.Map<List<DeliveryProviderReadDTO>>(providers.ToList());
        }

        public async Task<DeliveryProviderReadDTO> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            DeliveryProvider? provider = await unitOfWork.DeliveryProviders.GetByIdAsync(id, cancellationToken);
            if (provider == null) throw new NotFoundException($"DeliveryProvider not found with ID: {id}");

            return mapper.Map<DeliveryProviderReadDTO>(provider);
        }

        public async Task<DeliveryProviderReadDTO> CreateAsync(DeliveryProviderCreateDTO dto, CancellationToken cancellationToken = default)
        {
            bool exists = await unitOfWork.DeliveryProviders.ExistsByNameAsync(dto.Name, null, cancellationToken);
            if (exists) throw new AlreadyExistsException($"DeliveryProvider with name '{dto.Name}' already exists.");

            DeliveryProvider provider = mapper.Map<DeliveryProvider>(dto);

            if (dto.Icon != null) provider.IconUrl = await imageService.UploadAsync(dto.Icon, "delivery-providers");
            
            provider.Id = await unitOfWork.DeliveryProviders.AddAsync(provider, cancellationToken);
            await unitOfWork.CommitAsync();

            return mapper.Map<DeliveryProviderReadDTO>(provider);
        }

        public async Task<DeliveryProviderReadDTO> UpdateAsync(Guid id, DeliveryProviderUpdateDTO dto, CancellationToken cancellationToken = default)
        {
            DeliveryProvider? provider = await unitOfWork.DeliveryProviders.GetByIdAsync(id, cancellationToken);
            if (provider == null) throw new NotFoundException($"DeliveryProvider not found with ID: {id}");

            bool exists = await unitOfWork.DeliveryProviders.ExistsByNameAsync(dto.Name, id, cancellationToken);
            if (exists) throw new AlreadyExistsException($"DeliveryProvider with name '{dto.Name}' already exists.");

            if (dto.Icon != null)
            {
                if (!string.IsNullOrEmpty(provider.IconUrl)) await imageService.DeleteImageAsync(provider.IconUrl);
                provider.IconUrl = await imageService.UploadAsync(dto.Icon, "delivery-providers");
            }

            mapper.Map(dto, provider);
            await unitOfWork.DeliveryProviders.UpdateAsync(provider, cancellationToken);
            await unitOfWork.CommitAsync();

            return mapper.Map<DeliveryProviderReadDTO>(provider);
        }

        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            DeliveryProvider? provider = await unitOfWork.DeliveryProviders.GetByIdAsync(id, cancellationToken);
            if (provider == null) throw new NotFoundException($"DeliveryProvider not found with ID: {id}");

            if (!string.IsNullOrEmpty(provider.IconUrl)) await imageService.DeleteImageAsync(provider.IconUrl);

            await unitOfWork.DeliveryProviders.DeleteAsync(id, cancellationToken);
            await unitOfWork.CommitAsync();
        }
    }
}
