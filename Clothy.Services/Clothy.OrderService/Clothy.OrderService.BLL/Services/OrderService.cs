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
using MassTransit;
using Clothy.Shared.Events.OrderEvents;
using System.Security.Claims;
using Clothy.Shared.Helpers.JWT;
using Clothy.Shared.Events.UserEvents;
using MassTransit.Middleware;
using Grpc.Core;
using Clothy.Shared.Events.EmailEvents.OrderCreated;
using Clothy.Shared.Events.EmailEvents.OrderDelivered;

namespace Clothy.OrderService.BLL.Services
{
    public class OrderService : IOrderService
    {
        private IUnitOfWork unitOfWork;
        private IMapper mapper;
        private IEntityCacheService cacheService;
        private IPublishEndpoint publishEndpoint;
        private IEntityCacheInvalidationService<Order> cacheInvalidationService;
        private IOrderItemValidatorGrpcClient validatorGrpcClient;
        private IBasketGrpcClient basketGrpcClient;
        private IUserClaimsExtractor userClaimsExtractor;
        private ILogger<OrderService> logger;

        private const int MAX_CASHED_PAGES = 3;
        private static TimeSpan MEMORY_TTL_ORDER_DETAIL = TimeSpan.FromMinutes(30);
        private static TimeSpan REDIS_TTL_ORDER_DETAIL = TimeSpan.FromHours(2);
        private static TimeSpan MEMORY_TTL_PAGED_ORDERS = TimeSpan.FromMinutes(10);
        private static TimeSpan REDIS_TTL_PAGED_ORDERS = TimeSpan.FromHours(1);

        private Counter<long> ordersCreated;
        private Histogram<double> operationLatency;

        public OrderService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IEntityCacheService cacheService,
            IEntityCacheInvalidationService<Order> cacheInvalidationService,
            IOrderItemValidatorGrpcClient validatorGrpcClient,
            IBasketGrpcClient basketGrpcClient,
            Meter meter,
            IPublishEndpoint publishEndpoint,
            IUserClaimsExtractor userClaimsExtractor,
            ILogger<OrderService> logger)
        {
            this.unitOfWork = unitOfWork;
            this.mapper = mapper;
            this.cacheService = cacheService;
            this.cacheInvalidationService = cacheInvalidationService;
            this.validatorGrpcClient = validatorGrpcClient;
            this.basketGrpcClient = basketGrpcClient;
            this.publishEndpoint = publishEndpoint;
            this.userClaimsExtractor = userClaimsExtractor;
            this.logger = logger;

            ordersCreated = meter.CreateCounter<long>(
                "clothy.orderservice.orders.created-succesfully",
                "items",
                "Total count of created orders");
            operationLatency = meter.CreateHistogram<double>(
                "clothy.orderservice.orders.operation-duration",
                "ms",
                "Duration of created orders");
        }

        public async Task<OrderDetailDTO> CreateAsync(OrderCreateDTO dto, ClaimsPrincipal claimsPrincipal, CancellationToken cancellationToken = default)
        {
            Stopwatch stopwatch = new Stopwatch();
            string statusType = "Success";
            string failureReason = string.Empty;
            stopwatch.Start();

            try
            {
                Guid userId = userClaimsExtractor.GetUserId(claimsPrincipal);

                logger.LogInformation("Starting order creation for user: {UserId}", userId);

                GetUserBasketResponse basketResponse = await basketGrpcClient.GetUserBasketAsync(userId);

                if (basketResponse.Items == null || basketResponse.Items.Count == 0)
                {
                    logger.LogWarning("Basket is empty for user: {UserId}", userId);
                    throw new ValidationFailedException("Basket is empty. Cannot create order without items.");
                }

                logger.LogInformation("Retrieved basket for user: {UserId} with {ItemCount} items", userId, basketResponse.Items.Count);

                List<OrderItemToValidate> targetValidateRequest = basketResponse.Items.Select(p => new OrderItemToValidate
                {
                    ClotheId = p.ClotheId,
                    ColorId = p.ColorId,
                    SizeId = p.SizeId,
                    Quantity = p.Quantity
                }).ToList();

                logger.LogInformation("Validating {ItemCount} items before order creation", targetValidateRequest.Count);

                ValidateOrderItemsResponse validationResponse = await validatorGrpcClient.ValidateOrderItemsAsync(targetValidateRequest);

                for (int i = 0; i < validationResponse.Results.Count; i++)
                {
                    ValidateOrderItemResponse validationResult = validationResponse.Results[i];
                    if (!validationResult.IsValid)
                    {
                        logger.LogWarning("Order item validation failed: {ErrorMessage}", validationResult.ErrorMessage);
                        throw new ValidationFailedException($"Order item validation failed: {validationResult.ErrorMessage}");
                    }
                }

                logger.LogInformation("All items validated successfully");

                OrderStatus? pendingStatus = await unitOfWork.OrderStatuses.GetByNameAsync("Pending", cancellationToken);
                if (pendingStatus == null) throw new NotFoundException("Pending status not found");

                Order order = new Order
                {
                    UserId = userId,
                    UserFirstName = userClaimsExtractor.GetFirstName(claimsPrincipal),
                    UserLastName = userClaimsExtractor.GetLastName(claimsPrincipal),
                    CreatedAt = DateTime.UtcNow.ToUniversalTime(),
                    StatusId = pendingStatus.Id
                };

                await unitOfWork.Orders.AddAsync(order, cancellationToken);

                for (int i = 0; i < validationResponse.Results.Count; i++)
                {
                    ValidateOrderItemResponse validationResult = validationResponse.Results[i];
                    BasketItemMessage basketItem = basketResponse.Items[i];

                    OrderItem orderItem = new OrderItem
                    {
                        ClotheId = Guid.Parse(basketItem.ClotheId),
                        SizeId = Guid.Parse(basketItem.SizeId),
                        ColorId = Guid.Parse(basketItem.ColorId),
                        ClotheName = validationResult.ClotheName,
                        Price = decimal.Parse(validationResult.Price),
                        MainPhoto = validationResult.MainPhotoUrl,
                        SizeName = validationResult.SizeName,
                        HexCode = validationResult.ColorHexCode,
                        Quantity = basketItem.Quantity,
                        CreatedAt = DateTime.UtcNow.ToUniversalTime(),    
                        OrderId = order.Id
                    };
                    await unitOfWork.OrderItems.AddAsync(orderItem, cancellationToken);
                }

                PickupPoints? pickupPoints = await unitOfWork.PickupPoint.GetByIdAsync(dto.PickupPointId, cancellationToken);
                if (pickupPoints == null)
                {
                    throw new NotFoundException($"PickupPoint not found with ID: {dto.PickupPointId}");
                }

                DeliveryDetail delivery = mapper.Map<DeliveryDetail>(dto);
                delivery.CreatedAt = DateTime.UtcNow.ToUniversalTime();
                delivery.OrderId = order.Id;
                await unitOfWork.DeliveryDetails.AddAsync(delivery, cancellationToken);

                await unitOfWork.CommitAsync();

                logger.LogInformation("Order created successfully: {OrderId}", order.Id);

                ordersCreated.Add(1,
                    new KeyValuePair<string, object?>("status", "Pending"),
                    new KeyValuePair<string, object?>("pickupPointAddress", pickupPoints.Address));

                await cacheInvalidationService.InvalidateAllAsync();

                operationLatency.Record(
                    stopwatch.ElapsedMilliseconds,
                    new KeyValuePair<string, object?>("operation", "order_create"),
                    new KeyValuePair<string, object?>("status", statusType),
                    new KeyValuePair<string, object?>("failureReason", failureReason),
                    new KeyValuePair<string, object?>("itemsCount", basketResponse.Items.Count)
                );

                OrderCreatedEvent orderCreatedEvent = new OrderCreatedEvent
                {
                    OrderId = order.Id,
                    Items = basketResponse.Items.Select(item => new OrderItemEvent
                    {
                        ClotheId = Guid.Parse(item.ClotheId),
                        ColorId = Guid.Parse(item.ColorId),
                        SizeId = Guid.Parse(item.SizeId),
                        Quantity = item.Quantity,
                    }).ToList()
                };
                await publishEndpoint.Publish(orderCreatedEvent, cancellationToken);
                await basketGrpcClient.ClearUserBasketAsync(userId);

                logger.LogInformation("Cleared basket for user: {UserId} after order creation", userId);

                OrderDetailDTO? createdOrder = await GetByIdAsync(order.Id, cancellationToken: cancellationToken);

                OrderCreatedEmailEvent orderCreatedEmailEvent = new OrderCreatedEmailEvent()
                {
                    OrderId = createdOrder.Id,
                    UserEmail = userClaimsExtractor.GetEmail(claimsPrincipal),
                    Items = createdOrder.Items.Select(orderItem => new OrderItemEmailEvent
                    {
                        ClotheName = orderItem.ClotheName,
                        Size = orderItem.SizeName,
                        Color = orderItem.HexCode,
                        Quantity = orderItem.Quantity,
                        Price = orderItem.Price
                    }).ToList(),
                    TotalPrice = createdOrder.TotalPrice,
                };
                await publishEndpoint.Publish(orderCreatedEmailEvent, cancellationToken);

                return createdOrder;
            }
            catch (RpcException rpcEx)
            {
                failureReason = "gRPC_Error";
                statusType = "Failed";
                logger.LogError(rpcEx, "gRPC error during order creation");

                operationLatency.Record(
                    stopwatch.ElapsedMilliseconds,
                    new KeyValuePair<string, object?>("operation", "order_create"),
                    new KeyValuePair<string, object?>("status", "failed"),
                    new KeyValuePair<string, object?>("failureReason", failureReason),
                    new KeyValuePair<string, object?>("itemsCount", 0)
                );

                throw new ValidationFailedException($"Failed to communicate with services: {rpcEx.Status.Detail}");
            }
            catch (ValidationFailedException)
            {
                throw;
            }
            catch (NotFoundException)
            {
                throw;
            }
            catch (Exception ex)
            {
                failureReason = ex.GetType().Name;
                statusType = "Failed";
                logger.LogError(ex, "Unexpected error during order creation");

                operationLatency.Record(
                    stopwatch.ElapsedMilliseconds,
                    new KeyValuePair<string, object?>("operation", "order_create"),
                    new KeyValuePair<string, object?>("status", "failed"),
                    new KeyValuePair<string, object?>("failureReason", failureReason),
                    new KeyValuePair<string, object?>("itemsCount", 0)
                );

                throw;
            }
            finally
            {
                stopwatch.Stop();
            }
        }

        public async Task<OrderDetailDTO> GetByIdAsync(Guid id, ClaimsPrincipal? claimsPrincipal = null, CancellationToken cancellationToken = default)
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

                    if(claimsPrincipal != null)
                    {
                        bool isAdmin = userClaimsExtractor.IsInRole(claimsPrincipal, "Admin");
                        bool isManager = userClaimsExtractor.IsInRole(claimsPrincipal, "Manager");
                        Guid userId = userClaimsExtractor.GetUserId(claimsPrincipal);

                        if (!isAdmin && !isManager && orderData.UserId != userId) throw new ForbiddenException("You do not have access to this order.");
                    }

                    return mapper.Map<OrderDetailDTO>(orderData);
                },
                MEMORY_TTL_ORDER_DETAIL,
                REDIS_TTL_ORDER_DETAIL
            );

            return cached!;
        }

        public async Task<PagedList<OrderReadDTO>> GetPagedAsync(OrderFilterDTO filter, ClaimsPrincipal? user, CancellationToken cancellationToken = default)
        {
            if (user != null && !user.IsInRole("Admin") && !user.IsInRole("Manager")) filter.UserId = userClaimsExtractor.GetUserId(user);
           
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

            OrderDetailDTO orderDetailDTO = await GetByIdAsync(updatedOrder.Id, cancellationToken: cancellationToken);

            if(orderDetailDTO?.Status?.Name?.ToLower() == "completed" || orderDetailDTO?.Status?.Name?.ToLower() == "delivered" || orderDetailDTO?.Status?.Name?.ToLower() == "shipped")
            {
                OrderDeliveredEmailEvent orderDeliveredEmailEvent = new OrderDeliveredEmailEvent()
                {
                    OrderId = orderDetailDTO.Id,
                    Email = orderDetailDTO?.DeliveryDetail?.Email
                };
                await publishEndpoint.Publish(orderDeliveredEmailEvent, cancellationToken);
            }

            return orderDetailDTO;
        }

        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            Order? order = await unitOfWork.Orders.GetByIdAsync(id, cancellationToken);
            if (order == null) throw new NotFoundException($"Order not found with ID: {id}");

            await unitOfWork.Orders.DeleteAsync(order.Id, cancellationToken);
            await unitOfWork.CommitAsync();

            await cacheInvalidationService.InvalidateByIdAsync(id);
        }

        public async Task HandleUserUpdatedEventAsync(UserUpdatedEvent userUpdatedEvent)
        {
            await unitOfWork.Orders.UpdateUserNameAsync(userUpdatedEvent.UserId, userUpdatedEvent.FirstName, userUpdatedEvent.LastName);
            await unitOfWork.CommitAsync();

            await cacheInvalidationService.InvalidateAllAsync();
        }
    }
}