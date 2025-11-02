using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Clothy.OrderService.BLL.DTOs.OrderDTOs;
using Clothy.OrderService.BLL.Interfaces;
using Clothy.OrderService.DAL.UOW;
using Clothy.OrderService.Domain.Entities;
using Clothy.OrderService.Domain.Entities.AdditionalEntities;
using Clothy.Shared.Exceptions;
using Clothy.Shared.Helpers;
using Clothy.OrderService.DAL.FilterDTOs;

namespace Clothy.OrderService.BLL.Services
{
    public class OrderService : IOrderService
    {
        private IUnitOfWork unitOfWork;
        private IMapper mapper;

        public OrderService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            this.unitOfWork = unitOfWork;
            this.mapper = mapper;
        }

        public async Task<OrderDetailDTO> CreateAsync(OrderCreateDTO dto, CancellationToken cancellationToken = default)
        {
            OrderStatus? pendingStatus = await unitOfWork.OrderStatuses.GetByNameAsync("Pending", cancellationToken);
            if (pendingStatus == null) throw new NotFoundException("Pending status not found");

            Order order = mapper.Map<Order>(dto);
            order.StatusId = pendingStatus.Id;
            
            await unitOfWork.Orders.AddAsync(order, cancellationToken); 

            foreach (var itemDto in dto.Items)
            {
                OrderItem item = mapper.Map<OrderItem>(itemDto);
                item.OrderId = order.Id;
                await unitOfWork.OrderItems.AddAsync(item, cancellationToken);
            }

            DeliveryDetail delivery = mapper.Map<DeliveryDetail>(dto.DeliveryDetail);
            delivery.OrderId = order.Id;
            await unitOfWork.DeliveryDetails.AddAsync(delivery, cancellationToken);

            await unitOfWork.CommitAsync();

            return await GetByIdAsync(order.Id, cancellationToken);
        }

        public async Task<OrderDetailDTO> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            OrderWithDetailsData? orderData = await unitOfWork.Orders.GetByIdWithDetailsAsync(id, cancellationToken);
            if (orderData == null) throw new NotFoundException($"Order not found with ID: {id}");

            return mapper.Map<OrderDetailDTO>(orderData);
        }

        public async Task<PagedList<OrderReadDTO>> GetPagedAsync(OrderFilterDTO filter, CancellationToken cancellationToken = default)
        {
            var (orders, totalCount) = await unitOfWork.Orders.GetPagedAsync(filter, cancellationToken);
            List<OrderReadDTO> ordersDTO = mapper.Map<List<OrderReadDTO>>(orders);
 
            return new PagedList<OrderReadDTO>(ordersDTO, totalCount, filter.PageNumber, filter.PageSize);
        }

        public async Task<OrderDetailDTO> UpdateStatusAsync(Guid id, OrderUpdateStatusDTO dto, CancellationToken cancellationToken = default)
        {
            Order? order = await unitOfWork.Orders.GetByIdAsync(id, cancellationToken);
            if (order == null) throw new NotFoundException($"Order not found with ID: {id}");

            OrderStatus? status = await unitOfWork.OrderStatuses.GetByIdAsync(dto.StatusId, cancellationToken);
            if (status == null) throw new NotFoundException($"Order status not found with ID: {dto.StatusId}");

            order.StatusId = status.Id;

            Order? updatedOrder = await unitOfWork.Orders.UpdateAsync(order, cancellationToken);
            await unitOfWork.CommitAsync();

            return await GetByIdAsync(updatedOrder.Id, cancellationToken);
        }

        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            Order? order = await unitOfWork.Orders.GetByIdAsync(id, cancellationToken);
            if (order == null) throw new NotFoundException($"Order not found with ID: {id}");

            await unitOfWork.Orders.DeleteAsync(order.Id, cancellationToken);
            await unitOfWork.CommitAsync();
        }
    }
}