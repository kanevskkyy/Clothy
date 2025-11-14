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
using Clothy.Shared.Helpers;
using Clothy.OrderService.DAL.FilterDTOs;
using Clothy.Shared.Cache.Interfaces;
using Microsoft.Extensions.Logging;
using Clothy.Shared.Helpers.Exceptions;
using Clothy.OrderService.gRPC.Client.Services.Interfaces;
using System.Diagnostics.Metrics;
using System.Diagnostics;

namespace Clothy.OrderService.BLL.Services
{
    public class OrderService : IOrderService
    {
        private IUnitOfWork unitOfWork;
        private IMapper mapper;
        private IEntityCacheService cacheService;
        private IEntityCacheInvalidationService<Order> cacheInvalidationService;
        private IOrderItemValidatorGrpcClient validatorGrpcClient;
        private const int MAX_CASHED_PAGES = 3;

        private static TimeSpan MEMORY_TTL_ORDER_DETAIL = TimeSpan.FromMinutes(30);
        private static TimeSpan REDIS_TTL_ORDER_DETAIL = TimeSpan.FromHours(2);
        private static TimeSpan MEMORY_TTL_PAGED_ORDERS = TimeSpan.FromMinutes(10);
        private static TimeSpan REDIS_TTL_PAGED_ORDERS = TimeSpan.FromHours(1);

        private Counter<long> ordersCreated;
        private Histogram<double> operationLatency;

        public OrderService(IUnitOfWork unitOfWork, IMapper mapper, IEntityCacheService cacheService, IEntityCacheInvalidationService<Order> cacheInvalidationService, IOrderItemValidatorGrpcClient validatorGrpcClient, Meter meter)
        {
            this.unitOfWork = unitOfWork;
            this.mapper = mapper;
            this.cacheService = cacheService;
            this.cacheInvalidationService = cacheInvalidationService;
            this.validatorGrpcClient = validatorGrpcClient;
            ordersCreated = meter.CreateCounter<long>(
                "clothy.orderservice.orders.created-succesfully",
                "items",
                "Total count of created orders");
            operationLatency = meter.CreateHistogram<double>(
                "clothy.orderservice.orders.operation-duration",
                "ms",
                "Duration of created orders");
        }

        public async Task<OrderDetailDTO> CreateAsync(OrderCreateDTO dto, CancellationToken cancellationToken = default)
        {
            Stopwatch stopwatch = new Stopwatch();
            string statusType = "Success";
            string failureReason = string.Empty;
            stopwatch.Start();

            try
            {
                if (dto.Items == null || dto.Items.Count == 0) throw new ValidationFailedException("Order must contain at least one item");

                List<OrderItemToValidate> targetValidateRequest = dto.Items.Select(p => new OrderItemToValidate
                {
                    ClotheId = p.ClotheId.ToString(),
                    ColorId = p.ColorId.ToString(),
                    SizeId = p.SizeId.ToString(),
                    Quantity = p.Quantity
                }).ToList();

                var validationResponse = await validatorGrpcClient.ValidateOrderItemsAsync(targetValidateRequest);

                for (int i = 0; i < validationResponse.Results.Count; i++)
                {
                    ValidateOrderItemResponse validationResult = validationResponse.Results[i];

                    if (!validationResult.IsValid) throw new ValidationFailedException($"Order item validation failed: {validationResult.ErrorMessage}");
                }

                OrderStatus? pendingStatus = await unitOfWork.OrderStatuses.GetByNameAsync("Pending", cancellationToken);
                if (pendingStatus == null) throw new NotFoundException("Pending status not found");

                Order order = mapper.Map<Order>(dto);
                order.StatusId = pendingStatus.Id;
                await unitOfWork.Orders.AddAsync(order, cancellationToken);

                for (int i = 0; i < validationResponse.Results.Count; i++)
                {
                    ValidateOrderItemResponse validationResult = validationResponse.Results[i];
                    OrderItem orderItem = new OrderItem
                    {
                        ClotheId = dto.Items[i].ClotheId,
                        SizeId = dto.Items[i].SizeId,
                        ColorId = dto.Items[i].ColorId,
                        ClotheName = validationResult.ClotheName,
                        Price = decimal.Parse(validationResult.Price),
                        MainPhoto = validationResult.MainPhotoUrl,
                        SizeName = validationResult.SizeName,
                        HexCode = validationResult.ColorHexCode,
                        Quantity = dto.Items[i].Quantity,
                        OrderId = order.Id
                    };
                    await unitOfWork.OrderItems.AddAsync(orderItem, cancellationToken);
                }

                PickupPoints? pickupPoints = await unitOfWork.PickupPoint.GetByIdAsync(dto.DeliveryDetail.PickupPointId, cancellationToken);

                if (pickupPoints == null) throw new NotFoundException($"PickupPoint not found with ID: {dto.DeliveryDetail.PickupPointId}");

                DeliveryDetail delivery = mapper.Map<DeliveryDetail>(dto.DeliveryDetail);
                delivery.OrderId = order.Id;
                await unitOfWork.DeliveryDetails.AddAsync(delivery, cancellationToken);

                await unitOfWork.CommitAsync();

                ordersCreated.Add(1,
                    new KeyValuePair<string, object?>("status", "Pending"),
                    new KeyValuePair<string, object?>("pickupPointAddress", pickupPoints.Address));

                await cacheInvalidationService.InvalidateAllAsync();

                operationLatency.Record(
                    stopwatch.ElapsedMilliseconds,
                    new KeyValuePair<string, object?>("operation", "order_create"),
                    new KeyValuePair<string, object?>("status", statusType),
                    new KeyValuePair<string, object?>("failureReason", failureReason),
                    new KeyValuePair<string, object?>("itemsCount", dto.Items.Count)
                );

                return await GetByIdAsync(order.Id, cancellationToken);
            }
            catch (Exception ex)
            {
                operationLatency.Record(
                    stopwatch.ElapsedMilliseconds,
                    new KeyValuePair<string, object?>("operation", "order_create"),
                    new KeyValuePair<string, object?>("status", "failed"),
                    new KeyValuePair<string, object?>("failureReason", ex.GetType().Name),
                    new KeyValuePair<string, object?>("itemsCount", dto.Items.Count)
                );
                throw;
            }
        }


        public async Task<OrderDetailDTO> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            string cacheKey = $"order:{id}";
            TimeSpan memoryTTL = TimeSpan.FromMinutes(30); 
            TimeSpan redisTTL = TimeSpan.FromHours(2);     

            OrderDetailDTO? cached = await cacheService.GetOrSetAsync(
                cacheKey,
                async () =>
                {
                    OrderWithDetailsData? orderData = await unitOfWork.Orders.GetByIdWithDetailsAsync(id, cancellationToken);
                    if (orderData == null) throw new NotFoundException($"Order not found with ID: {id}");
                    return mapper.Map<OrderDetailDTO>(orderData);
                },
                MEMORY_TTL_ORDER_DETAIL,
                REDIS_TTL_ORDER_DETAIL
            );

            return cached!;
        }

        public async Task<PagedList<OrderReadDTO>> GetPagedAsync(OrderFilterDTO filter, CancellationToken cancellationToken = default)
        {
            bool usePageCache = filter.StatusId.HasValue && filter.PageNumber <= MAX_CASHED_PAGES;

            if (usePageCache)
            {
                string cacheKey = $"orders:status:{filter.StatusId}:page:{filter.PageNumber}:size:{filter.PageSize}";
                TimeSpan memoryTTL = TimeSpan.FromMinutes(10);
                TimeSpan redisTTL = TimeSpan.FromHours(1);

                PagedList<OrderReadDTO>? cached = await cacheService.GetOrSetAsync(
                    cacheKey,
                    async () =>
                    {
                        var (orders, totalCount) = await unitOfWork.Orders.GetPagedAsync(filter, cancellationToken);
                        List<OrderReadDTO> ordersDTO = mapper.Map<List<OrderReadDTO>>(orders);
                        return new PagedList<OrderReadDTO>(ordersDTO, totalCount, filter.PageNumber, filter.PageSize);
                    },
                    MEMORY_TTL_PAGED_ORDERS,
                    REDIS_TTL_PAGED_ORDERS
                );

                return cached!;
            }
            else
            {
                var (orders, totalCount) = await unitOfWork.Orders.GetPagedAsync(filter, cancellationToken);
                List<OrderReadDTO> ordersDTO = mapper.Map<List<OrderReadDTO>>(orders);
                return new PagedList<OrderReadDTO>(ordersDTO, totalCount, filter.PageNumber, filter.PageSize);
            }
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

            await cacheInvalidationService.InvalidateByIdAsync(updatedOrder.Id);

            return await GetByIdAsync(updatedOrder.Id, cancellationToken);
        }

        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            Order? order = await unitOfWork.Orders.GetByIdAsync(id, cancellationToken);
            if (order == null) throw new NotFoundException($"Order not found with ID: {id}");

            await unitOfWork.Orders.DeleteAsync(order.Id, cancellationToken);
            await unitOfWork.CommitAsync();

            await cacheInvalidationService.InvalidateByIdAsync(id);
        }
    }
}