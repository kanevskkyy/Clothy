using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Clothy.OrderService.BLL.DTOs.PickupPointsDTOs;
using Clothy.OrderService.BLL.Interfaces;
using Clothy.OrderService.DAL.FilterDTOs;
using Clothy.OrderService.Domain.Entities;
using Clothy.Shared.Helpers.Exceptions;
using Clothy.Shared.Helpers;
using Clothy.OrderService.DAL.UOW;

namespace Clothy.OrderService.BLL.Services
{
    public class PickupPointService : IPickupPointService
    {
        private IUnitOfWork unitOfWork;
        private IMapper mapper;

        public PickupPointService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            this.unitOfWork = unitOfWork;
            this.mapper = mapper;
        }

        public async Task<PickupPointReadDTO> CreateAsync(PickupPointCreateDTO dto, CancellationToken cancellationToken = default)
        {
            bool exists = await unitOfWork.PickupPoint.ExistsByAddressAndProviderIdAsync(dto.Address, dto.DeliveryProviderId, cancellationToken: cancellationToken);
            if (exists) throw new AlreadyExistsException($"Pickup point with address {dto.Address} for this provider already exists");

            DeliveryProvider? deliveryProvider = await unitOfWork.DeliveryProviders.GetByIdAsync(dto.DeliveryProviderId, cancellationToken);
            if (deliveryProvider == null) throw new NotFoundException($"DeliveryProvider with ID: {dto.DeliveryProviderId}");

            PickupPoints entity = mapper.Map<PickupPoints>(dto);
            entity.Id = await unitOfWork.PickupPoint.AddAsync(entity, cancellationToken);

            await unitOfWork.CommitAsync();
            return mapper.Map<PickupPointReadDTO>(entity);
        }

        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            PickupPoints? pickupPoint = await unitOfWork.PickupPoint.GetByIdAsync(id, cancellationToken);
            if (pickupPoint == null) throw new NotFoundException($"PickupPoint not found with ID: {id}");

            await unitOfWork.PickupPoint.DeleteAsync(id, cancellationToken);
            await unitOfWork.CommitAsync();
        }

        public async Task<PickupPointReadDTO> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            PickupPoints? pickupPoint = await unitOfWork.PickupPoint.GetByIdAsync(id, cancellationToken);
            if (pickupPoint == null) throw new NotFoundException($"PickupPoint not found with ID: {id}");

            return mapper.Map<PickupPointReadDTO>(pickupPoint);
        }

        public async Task<PagedList<PickupPointReadDTO>> GetPagedAsync(PickupPointFilterDTO pickupPointFiler, CancellationToken cancellationToken = default)
        {
            var (items, totalCount) = await unitOfWork.PickupPoint.GetPagedAsync(pickupPointFiler, cancellationToken);

            List<PickupPointReadDTO> pickupPointsDtos = mapper.Map<List<PickupPointReadDTO>>(items);
            return new PagedList<PickupPointReadDTO>(pickupPointsDtos, totalCount, pickupPointFiler.PageNumber, pickupPointFiler.PageSize);
        }

        public async Task<PickupPointReadDTO> UpdateAsync(Guid id, PickupPointUpdateDTO pickupPointUpdateDto, CancellationToken cancellationToken = default)
        {
            PickupPoints? entity = await unitOfWork.PickupPoint.GetByIdAsync(id, cancellationToken);
            if (entity == null) throw new NotFoundException($"PickupPoint not found with ID: {id}");

            bool exists = await unitOfWork.PickupPoint.ExistsByAddressAndProviderIdAsync(pickupPointUpdateDto.Address, pickupPointUpdateDto.DeliveryProviderId, id, cancellationToken);
            if (exists) throw new AlreadyExistsException($"Pickup point with address: {pickupPointUpdateDto.Address} for this provider already exists");

            DeliveryProvider? provider = await unitOfWork.DeliveryProviders.GetByIdAsync(pickupPointUpdateDto.DeliveryProviderId, cancellationToken);
            if (provider == null) throw new NotFoundException($"DeliveryProvider not found with ID: {pickupPointUpdateDto.DeliveryProviderId}");

            mapper.Map(pickupPointUpdateDto, entity);
            await unitOfWork.PickupPoint.UpdateAsync(entity);
            await unitOfWork.CommitAsync();

            return mapper.Map<PickupPointReadDTO>(entity);
        }
    }
}
