using AutoMapper;
using Clothy.OrderService.BLL.DTOs.OrderDTOs;
using Clothy.OrderService.BLL.Interfaces;
using Clothy.OrderService.DAL.FilterDTOs;
using Clothy.OrderService.DAL.UOW;
using Clothy.OrderService.Domain.Entities;
using Clothy.OrderService.Domain.Entities.AdditionalEntities;
using Clothy.OrderService.gRPC.Client.Services.Interfaces;
using Clothy.Shared.Cache.Interfaces;
using Clothy.Shared.Events.EmailEvents.OrderCreated;
using Clothy.Shared.Events.EmailEvents.OrderDelivered;
using Clothy.Shared.Events.OrderEvents;
using Clothy.Shared.Events.PaymentEvents;
using Clothy.Shared.Helpers;
using Clothy.Shared.Helpers.Exceptions;
using Clothy.Shared.Helpers.JWT;
using Grpc.Core;
using MassTransit;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Security.Claims;

namespace Clothy.OrderService.BLL.Services
{
    public class OrderService : IOrderService
    {
        private IUnitOfWork unitOfWork;
        private IMapper mapper;
        private IEntityCacheService cacheService;
        private IPublishEndpoint publishEndpoint;
        private IEntityCacheInvalidationService<Order> orderInvalidationService;
        private IOrderItemValidatorGrpcClient validatorGrpcClient;
        private IBasketGrpcClient basketGrpcClient;
        private IUserClaimsExtractor userClaimsExtractor;
        private ILogger<OrderService> logger;

        private const int MAX_CASHED_PAGES = 3;
        private static TimeSpan MEMORY_TTL_ORDER_DETAIL = TimeSpan.FromMinutes(30);
        private static TimeSpan REDIS_TTL_ORDER_DETAIL = TimeSpan.FromHours(2);
        private static TimeSpan MEMORY_TTL_PAGED_ORDERS = TimeSpan.FromMinutes(10);
        private static TimeSpan REDIS_TTL_PAGED_ORDERS = TimeSpan.FromHours(1);

        private const int TIME_FOR_PAY = 10;

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
            this.orderInvalidationService = cacheInvalidationService;
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

        public async Task<OrderDetailDTO> CreateAsync(OrderCreateDTO orderCreateDTO, ClaimsPrincipal claimsPrincipal, CancellationToken cancellationToken = default)
        {
            Stopwatch stopwatch = new Stopwatch();
            string statusType = "Success";
            string failureReason = string.Empty;
            stopwatch.Start();

            try
            {
                bool emailConfirmed = userClaimsExtractor.EmailConfirmed(claimsPrincipal);
                if (!emailConfirmed) throw new ValidationFailedException("You need to confirm your email before placing an order!");

                Guid userId = userClaimsExtractor.GetUserId(claimsPrincipal);

                logger.LogInformation("Starting order creation for user: {UserId}", userId);

                GetUserBasketResponse basketResponse = await basketGrpcClient.GetUserBasketAsync(userId);

                if (basketResponse.Items == null || basketResponse.Items.Count == 0)
                {
                    logger.LogWarning("Basket is empty for user: {UserId}", userId);
                    throw new ValidationFailedException("Basket is empty. Cannot create order without items.");
                }

                logger.LogInformation("Retrieved basket for user: {UserId} with {ItemCount} items", userId, basketResponse.Items.Count);

                List<OrderItemToValidate> targetValidateRequest = new List<OrderItemToValidate>();

                foreach (BasketItemMessage item in basketResponse.Items)
                {
                    int reservedQuantity = await unitOfWork.OrderReservation.GetReservedQuantityAsync(
                        Guid.Parse(item.ClotheId),
                        Guid.Parse(item.SizeId),
                        Guid.Parse(item.ColorId),
                        cancellationToken);

                    targetValidateRequest.Add(new OrderItemToValidate()
                    {
                        ClotheId = item.ClotheId,
                        SizeId = item.SizeId,
                        ColorId = item.ColorId,
                        Quantity = item.Quantity + reservedQuantity
                    });
                }

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

                OrderStatus? awaitingPaymentStatus = await unitOfWork.OrderStatuses.GetByNameAsync("Awaiting payment", cancellationToken);
                if (awaitingPaymentStatus == null) throw new NotFoundException("Awaiting payment status not found");

                bool hasOrdered = await unitOfWork.Orders.HasUserAlreadyOrderedAsync(userId, cancellationToken);
                decimal finalDiscount = hasOrdered ? 1.0m : 0.9m;
                if (!hasOrdered) logger.LogInformation("Applied 10% discount for first order of user {UserId}", userId);

                Order order = new Order
                {
                    UserId = userId,
                    UserFirstName = userClaimsExtractor.GetFirstName(claimsPrincipal),
                    UserLastName = userClaimsExtractor.GetLastName(claimsPrincipal),
                    UserEmail = userClaimsExtractor.GetEmail(claimsPrincipal),
                    CreatedAt = DateTime.UtcNow.ToUniversalTime(),
                    StatusId = awaitingPaymentStatus.Id
                };

                await unitOfWork.Orders.AddAsync(order, cancellationToken);

                DateTime now = DateTime.UtcNow.ToUniversalTime();
                DateTime expiredAt = now.AddMinutes(TIME_FOR_PAY);

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
                        Price = decimal.Parse(validationResult.Price) * finalDiscount,
                        MainPhoto = validationResult.MainPhotoUrl,
                        SizeName = validationResult.SizeName,
                        HexCode = validationResult.ColorHexCode,
                        Quantity = basketItem.Quantity,
                        CreatedAt = DateTime.UtcNow.ToUniversalTime(),
                        OrderId = order.Id
                    };
                    await unitOfWork.OrderItems.AddAsync(orderItem, cancellationToken);

                    OrderReservation orderReservation = new OrderReservation
                    {
                        OrderId = order.Id,
                        ClotheId = Guid.Parse(basketItem.ClotheId),
                        SizeId = Guid.Parse(basketItem.SizeId),
                        ColorId = Guid.Parse(basketItem.ColorId),
                        Quantity = basketItem.Quantity,
                        ReservedAt = now,
                        ExpiresAt = expiredAt,
                        IsActive = true
                    };
                    await unitOfWork.OrderReservation.AddAsync(orderReservation, cancellationToken);
                }

                PickupPoints? pickupPoints = await unitOfWork.PickupPoint.GetByIdAsync(orderCreateDTO.PickupPointId, cancellationToken);
                if (pickupPoints == null || !pickupPoints.IsActive) throw new NotFoundException($"PickupPoint not found with ID: {orderCreateDTO.PickupPointId}");

                DeliveryDetail delivery = mapper.Map<DeliveryDetail>(orderCreateDTO);
                delivery.CreatedAt = DateTime.UtcNow.ToUniversalTime();
                delivery.OrderId = order.Id;
                await unitOfWork.DeliveryDetails.AddAsync(delivery, cancellationToken);

                await unitOfWork.CommitAsync();

                logger.LogInformation("Order created successfully: {OrderId}", order.Id);

                ordersCreated.Add(1,
                    new KeyValuePair<string, object?>("status", "Pending"),
                    new KeyValuePair<string, object?>("pickupPointAddress", pickupPoints.Address));

                await orderInvalidationService.InvalidateAllAsync();

                operationLatency.Record(
                    stopwatch.ElapsedMilliseconds,
                    new KeyValuePair<string, object?>("operation", "order_create"),
                    new KeyValuePair<string, object?>("status", statusType),
                    new KeyValuePair<string, object?>("failureReason", failureReason),
                    new KeyValuePair<string, object?>("itemsCount", basketResponse.Items.Count)
                );

                OrderDetailDTO? createdOrder = await GetByIdAsync(order.Id, cancellationToken: cancellationToken);
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

        public async Task HandleOrderPaidEventAsync(OrderPaidEvent orderPaidEvent, CancellationToken cancellationToken = default)
        {
            OrderStatus? pendingStatus = await unitOfWork.OrderStatuses.GetByNameAsync("Pending", cancellationToken);
            if (pendingStatus == null) throw new NotFoundException("Pending status not found!");

            OrderUpdateStatusDTO orderUpdateStatusDTO = new OrderUpdateStatusDTO
            {
                StatusId = pendingStatus.Id
            };
            OrderDetailDTO orderDetailDTO = await UpdateStatusAsync(orderPaidEvent.OrderId, orderUpdateStatusDTO, cancellationToken);

            OrderCreatedEvent orderCreatedEvent = new OrderCreatedEvent
            {
                OrderId = orderDetailDTO.Id,
                Items = orderDetailDTO.Items.Select(item => new OrderItemEvent
                {
                    ClotheId = item.ClotheId,
                    ColorId = item.ColorId,
                    SizeId = item.SizeId,
                    Quantity = item.Quantity,
                }).ToList()
            };
            await publishEndpoint.Publish(orderCreatedEvent, cancellationToken);
            await basketGrpcClient.ClearUserBasketAsync(orderDetailDTO.UserId);

            logger.LogInformation("Cleared basket for user: {UserId} after order creation", orderDetailDTO.UserId);

            OrderCreatedEmailEvent orderCreatedEmailEvent = new OrderCreatedEmailEvent()
            {
                OrderId = orderDetailDTO.Id,
                UserEmail = orderDetailDTO.UserEmail,
                Items = orderDetailDTO.Items.Select(orderItem => new OrderItemEmailEvent
                {
                    ClotheName = orderItem.ClotheName,
                    Size = orderItem.SizeName,
                    Color = orderItem.HexCode,
                    Quantity = orderItem.Quantity,
                    Price = orderItem.Price
                }).ToList(),
                TotalPrice = orderDetailDTO.TotalPrice,
            };
            await publishEndpoint.Publish(orderCreatedEmailEvent, cancellationToken);

            List<OrderReservation> orderReservations = await unitOfWork.OrderReservation.GetByOrderIdAsync(orderCreatedEvent.OrderId, cancellationToken);
            foreach (OrderReservation reservation in orderReservations)
            {
                reservation.IsActive = false;
                await unitOfWork.OrderReservation.UpdateAsync(reservation, cancellationToken);
            }
            await unitOfWork.CommitAsync();
            await orderInvalidationService.InvalidateAllAsync();
            await orderInvalidationService.InvalidateByIdAsync(orderPaidEvent.OrderId);
        }

        public async Task<OrderDetailDTO> GetByIdAsync(Guid id, ClaimsPrincipal? claimsPrincipal = null, CancellationToken cancellationToken = default)
        {
            string cacheKey = $"order:{id}";

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

            if (claimsPrincipal != null)
            {
                bool isAdmin = userClaimsExtractor.IsInRole(claimsPrincipal, "Admin");
                bool isManager = userClaimsExtractor.IsInRole(claimsPrincipal, "Manager");
                Guid userId = userClaimsExtractor.GetUserId(claimsPrincipal);

                if (!isAdmin && !isManager && cached.UserId != userId) throw new ForbiddenException("You do not have access to this order.");
            }

            return cached!;
        }

        public async Task<PagedList<OrderReadDTO>?> GetPagedAsync(OrderFilterDTO filter, ClaimsPrincipal? user, CancellationToken cancellationToken = default)
        {
            if (user != null && !user.IsInRole("Admin") && !user.IsInRole("Manager"))
                filter.UserId = userClaimsExtractor.GetUserId(user);

            bool usePageCache = filter.StatusId.HasValue && filter.PageNumber <= MAX_CASHED_PAGES;

            if (usePageCache)
            {
                return await cacheService.GetOrSetAsync(
                    filter.ToCacheKey(),
                    async () => await FetchOrdersAsync(filter, cancellationToken),
                    MEMORY_TTL_PAGED_ORDERS,
                    REDIS_TTL_PAGED_ORDERS
                );
            }

            return await FetchOrdersAsync(filter, cancellationToken);
        }

        private async Task<PagedList<OrderReadDTO>> FetchOrdersAsync(OrderFilterDTO filter, CancellationToken cancellationToken)
        {
            var (orders, totalCount) = await unitOfWork.Orders.GetPagedAsync(filter, cancellationToken);
            
            List<OrderReadDTO> ordersDTO = mapper.Map<List<OrderReadDTO>>(orders);
            return new PagedList<OrderReadDTO>(ordersDTO, totalCount, filter.PageNumber, filter.PageSize);
        }


        public async Task<OrderDetailDTO> UpdateStatusAsync(Guid id, OrderUpdateStatusDTO orderUpdateStatusDTO, CancellationToken cancellationToken = default)
        {
            Order? order = await unitOfWork.Orders.GetByIdAsync(id, cancellationToken);
            if (order == null) throw new NotFoundException($"Order not found with ID: {id}");

            OrderStatus? status = await unitOfWork.OrderStatuses.GetByIdAsync(orderUpdateStatusDTO.StatusId, cancellationToken);
            if (status == null) throw new NotFoundException($"Order status not found with ID: {orderUpdateStatusDTO.StatusId}");

            order.StatusId = status.Id;

            Order? updatedOrder = await unitOfWork.Orders.UpdateAsync(order, cancellationToken);
            await unitOfWork.CommitAsync();

            await orderInvalidationService.InvalidateAllAsync();
            await orderInvalidationService.InvalidateByIdAsync(updatedOrder.Id);

            OrderDetailDTO orderDetailDTO = await GetByIdAsync(updatedOrder.Id, cancellationToken: cancellationToken);

            if (orderDetailDTO?.Status?.Name?.ToLower() == "completed" || orderDetailDTO?.Status?.Name?.ToLower() == "delivered" || orderDetailDTO?.Status?.Name?.ToLower() == "shipped")
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

            await orderInvalidationService.InvalidateByIdAsync(id);
            await orderInvalidationService.InvalidateAllAsync();
        }
    }
}