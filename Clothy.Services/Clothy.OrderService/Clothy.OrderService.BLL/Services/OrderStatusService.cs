using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Clothy.OrderService.BLL.DTOs.OrderStatusDTOs;
using Clothy.OrderService.BLL.Interfaces;
using Clothy.OrderService.DAL.UOW;
using Clothy.OrderService.Domain.Entities;
using Clothy.Shared.Exceptions;
using Clothy.Shared.Helpers.CloudinaryConfig;

namespace Clothy.OrderService.BLL.Services
{
    public class OrderStatusService : IOrderStatusService
    {
        private IUnitOfWork unitOfWork;
        private IMapper mapper;
        private IImageService imageService;

        public OrderStatusService(IUnitOfWork unitOfWork, IMapper mapper, IImageService imageService)
        {
            this.unitOfWork = unitOfWork;
            this.mapper = mapper;
            this.imageService = imageService;
        }

        public async Task<List<OrderStatusReadDTO>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            IEnumerable<OrderStatus> statuses = await unitOfWork.OrderStatuses.GetAllAsync(cancellationToken);
            return mapper.Map<List<OrderStatusReadDTO>>(statuses.ToList());
        }

        public async Task<OrderStatusReadDTO> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            OrderStatus? status = await unitOfWork.OrderStatuses.GetByIdAsync(id, cancellationToken);
            if (status == null) throw new NotFoundException($"OrderStatus not found with ID: {id}");

            return mapper.Map<OrderStatusReadDTO>(status);
        }

        public async Task<OrderStatusReadDTO> CreateAsync(OrderStatusCreateDTO dto, CancellationToken cancellationToken = default)
        {
            bool exists = await unitOfWork.OrderStatuses.ExistsByNameAsync(dto.Name, null, cancellationToken);
            if (exists) throw new AlreadyExistsException($"OrderStatus with name '{dto.Name}' already exists.");

            OrderStatus status = mapper.Map<OrderStatus>(dto);
            status.IconUrl = await imageService.UploadAsync(dto.Icon, "order-statuses");

            status.Id = await unitOfWork.OrderStatuses.AddAsync(status, cancellationToken);
            await unitOfWork.CommitAsync();

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

            return mapper.Map<OrderStatusReadDTO>(status);
        }

        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            OrderStatus? status = await unitOfWork.OrderStatuses.GetByIdAsync(id, cancellationToken);
            if (status == null) throw new NotFoundException($"OrderStatus not found with ID: {id}");

            if (!string.IsNullOrEmpty(status.IconUrl)) await imageService.DeleteImageAsync(status.IconUrl);

            await unitOfWork.OrderStatuses.DeleteAsync(id, cancellationToken);
            await unitOfWork.CommitAsync();
        }
    }
}
