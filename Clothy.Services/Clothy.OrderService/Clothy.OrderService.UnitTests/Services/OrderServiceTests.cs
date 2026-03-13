using AutoMapper;
using Clothy.OrderService.BLL.DTOs.OrderDTOs;
using FluentAssertions;
using Grpc.Core;
using MassTransit;
using Microsoft.Extensions.Logging;
using Moq;
using System.Diagnostics.Metrics;
using System.Security.Claims;
using Clothy.OrderService.BLL.DTOs.DeliveryDetailDTOs;
using Clothy.OrderService.BLL.DTOs.OrderItemDTOs;
using Clothy.OrderService.DAL.Interfaces;
using Clothy.OrderService.DAL.UOW;
using Clothy.OrderService.Domain.Entities;
using Clothy.OrderService.Domain.Entities.AdditionalEntities;
using Clothy.OrderService.gRPC.Client.Services.Interfaces;
using Clothy.Shared.Cache.Interfaces;
using Clothy.Shared.Events.EmailEvents;
using Clothy.Shared.Events.OrderEvents;
using Clothy.Shared.Events.PaymentEvents;
using Clothy.Shared.Helpers.Exceptions;
using Clothy.Shared.Helpers.JWT;
using Xunit;

namespace Clothy.OrderService.UnitTests.Services;

public class OrderServiceTests
{
    private Mock<IUnitOfWork> unitOfWorkMock;
    private Mock<IOrderRepository> orderRepoMock;
    private Mock<IOrderItemRepository> orderItemRepoMock;
    private Mock<IOrderReservationRepository> orderReservationRepoMock;
    private Mock<IPickupPointRepository> pickupPointRepoMock;
    private Mock<IDeliveryDetailRepository> deliveryDetailRepoMock;
    private Mock<IMapper> mapperMock;
    private Mock<IEntityCacheService> cacheServiceMock;
    private Mock<IEntityCacheInvalidationService<Order>> orderInvalidationMock;
    private Mock<IOrderItemValidatorGrpcClient> validatorGrpcClientMock;
    private Mock<IBasketGrpcClient> basketGrpcClientMock;
    private Mock<IPublishEndpoint> publishEndpointMock;
    private Mock<IUserClaimsExtractor> userClaimsExtractorMock;
    private Mock<ILogger<BLL.Services.OrderService>> loggerMock;

    private BLL.Services.OrderService orderService;

    public OrderServiceTests()
    {
        unitOfWorkMock = new Mock<IUnitOfWork>();
        orderRepoMock = new Mock<IOrderRepository>();
        orderItemRepoMock = new Mock<IOrderItemRepository>();
        orderReservationRepoMock = new Mock<IOrderReservationRepository>();
        pickupPointRepoMock = new Mock<IPickupPointRepository>();
        deliveryDetailRepoMock = new Mock<IDeliveryDetailRepository>();
        mapperMock = new Mock<IMapper>();
        cacheServiceMock = new Mock<IEntityCacheService>();
        orderInvalidationMock = new Mock<IEntityCacheInvalidationService<Order>>();
        validatorGrpcClientMock = new Mock<IOrderItemValidatorGrpcClient>();
        basketGrpcClientMock = new Mock<IBasketGrpcClient>();
        publishEndpointMock = new Mock<IPublishEndpoint>();
        userClaimsExtractorMock = new Mock<IUserClaimsExtractor>();
        loggerMock = new Mock<ILogger<BLL.Services.OrderService>>();

        unitOfWorkMock.Setup(u => u.Orders).Returns(orderRepoMock.Object);
        unitOfWorkMock.Setup(u => u.OrderItems).Returns(orderItemRepoMock.Object);
        unitOfWorkMock.Setup(u => u.OrderReservation).Returns(orderReservationRepoMock.Object);
        unitOfWorkMock.Setup(u => u.PickupPoint).Returns(pickupPointRepoMock.Object);
        unitOfWorkMock.Setup(u => u.DeliveryDetails).Returns(deliveryDetailRepoMock.Object);

        Meter meter = new Meter("test.order.meter");

        orderService = new BLL.Services.OrderService(
            unitOfWorkMock.Object,
            mapperMock.Object,
            cacheServiceMock.Object,
            orderInvalidationMock.Object,
            validatorGrpcClientMock.Object,
            basketGrpcClientMock.Object,
            meter,
            publishEndpointMock.Object,
            userClaimsExtractorMock.Object,
            loggerMock.Object);
    }

    private void SetupCacheMiss<T>()
    {
        cacheServiceMock
            .Setup(c => c.GetOrSetAsync(
                It.IsAny<string>(),
                It.IsAny<Func<Task<T>>>(),
                It.IsAny<TimeSpan>(),
                It.IsAny<TimeSpan>()))
            .Returns<string, Func<Task<T>>, TimeSpan, TimeSpan>(
                (key, factory, m, r) => factory());
    }

    private ClaimsPrincipal BuildClaims(Guid userId, bool emailConfirmed = true)
    {
        ClaimsPrincipal claims = new ClaimsPrincipal();
        userClaimsExtractorMock.Setup(e => e.GetUserId(claims)).Returns(userId);
        userClaimsExtractorMock.Setup(e => e.EmailConfirmed(claims)).Returns(emailConfirmed);
        userClaimsExtractorMock.Setup(e => e.GetEmail(claims)).Returns("user@test.com");
        userClaimsExtractorMock.Setup(e => e.GetFirstName(claims)).Returns("John");
        userClaimsExtractorMock.Setup(e => e.GetLastName(claims)).Returns("Doe");
        return claims;
    }

    private GetUserBasketResponse BuildBasketResponse(int itemCount = 1)
    {
        GetUserBasketResponse response = new GetUserBasketResponse();
        for (int i = 0; i < itemCount; i++)
        {
            response.Items.Add(new BasketItemMessage
            {
                ClotheId = Guid.NewGuid().ToString(),
                SizeId = Guid.NewGuid().ToString(),
                ColorId = Guid.NewGuid().ToString(),
                Quantity = 1
            });
        }
        return response;
    }

    private ValidateOrderItemsResponse BuildValidateResponse(int count = 1)
    {
        ValidateOrderItemsResponse response = new ValidateOrderItemsResponse();
        for (int i = 0; i < count; i++)
        {
            response.Results.Add(new ValidateOrderItemResponse
            {
                IsValid = true,
                ClotheName = "Test Clothe",
                Price = "100.00",
                MainPhotoUrl = "https://photo.jpg",
                SizeName = "M",
                ColorHexCode = "#FF0000"
            });
        }
        return response;
    }

    [Fact]
    public async Task CreateAsync_WhenEmailNotConfirmed_ThrowsValidationFailedException()
    {
        Guid userId = Guid.NewGuid();
        ClaimsPrincipal claims = BuildClaims(userId, emailConfirmed: false);
        OrderCreateDTO createDTO = new OrderCreateDTO { PickupPointId = Guid.NewGuid() };

        Func<Task> act = async () => await orderService.CreateAsync(createDTO, claims);

        await act.Should().ThrowAsync<ValidationFailedException>()
            .WithMessage("*confirm your email*");
    }

    [Fact]
    public async Task CreateAsync_WhenBasketIsEmpty_ThrowsValidationFailedException()
    {
        Guid userId = Guid.NewGuid();
        ClaimsPrincipal claims = BuildClaims(userId);
        OrderCreateDTO createDTO = new OrderCreateDTO { PickupPointId = Guid.NewGuid() };

        basketGrpcClientMock
            .Setup(c => c.GetUserBasketAsync(userId))
            .ReturnsAsync(new GetUserBasketResponse());

        Func<Task> act = async () => await orderService.CreateAsync(createDTO, claims);

        await act.Should().ThrowAsync<ValidationFailedException>()
            .WithMessage("*Basket is empty*");
    }

    [Fact]
    public async Task CreateAsync_WhenItemValidationFails_ThrowsValidationFailedException()
    {
        Guid userId = Guid.NewGuid();
        ClaimsPrincipal claims = BuildClaims(userId);
        OrderCreateDTO createDTO = new OrderCreateDTO { PickupPointId = Guid.NewGuid() };

        basketGrpcClientMock
            .Setup(c => c.GetUserBasketAsync(userId))
            .ReturnsAsync(BuildBasketResponse());

        orderReservationRepoMock
            .Setup(r => r.GetReservedQuantityAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        ValidateOrderItemsResponse invalidResponse = new ValidateOrderItemsResponse();
        invalidResponse.Results.Add(new ValidateOrderItemResponse
        {
            IsValid = false,
            ErrorMessage = "Not enough stock"
        });

        validatorGrpcClientMock
            .Setup(c => c.ValidateOrderItemsAsync(It.IsAny<List<OrderItemToValidate>>()))
            .ReturnsAsync(invalidResponse);

        Func<Task> act = async () => await orderService.CreateAsync(createDTO, claims);

        await act.Should().ThrowAsync<ValidationFailedException>()
            .WithMessage("*Not enough stock*");
    }

    [Fact]
    public async Task CreateAsync_WhenRpcExceptionThrown_ThrowsValidationFailedException()
    {
        Guid userId = Guid.NewGuid();
        ClaimsPrincipal claims = BuildClaims(userId);
        OrderCreateDTO createDTO = new OrderCreateDTO { PickupPointId = Guid.NewGuid() };

        basketGrpcClientMock
            .Setup(c => c.GetUserBasketAsync(userId))
            .ReturnsAsync(BuildBasketResponse());

        orderReservationRepoMock
            .Setup(r => r.GetReservedQuantityAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        validatorGrpcClientMock
            .Setup(c => c.ValidateOrderItemsAsync(It.IsAny<List<OrderItemToValidate>>()))
            .ThrowsAsync(new RpcException(new Status(StatusCode.Unavailable, "Service unavailable")));

        Func<Task> act = async () => await orderService.CreateAsync(createDTO, claims);

        await act.Should().ThrowAsync<ValidationFailedException>()
            .WithMessage("*Failed to communicate with services*");
    }

    [Fact]
    public async Task GetByIdAsync_WhenOrderNotFound_ThrowsNotFoundException()
    {
        Guid id = Guid.NewGuid();

        SetupCacheMiss<OrderDetailDTO?>();

        orderRepoMock
            .Setup(r => r.GetByIdWithDetailsAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((OrderWithDetailsData?)null);

        Func<Task> act = async () => await orderService.GetByIdAsync(id);

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"*{id}*");
    }

    [Fact]
    public async Task GetByIdAsync_WhenOrderExists_ReturnsOrderDetailDTO()
    {
        Guid id = Guid.NewGuid();
        OrderWithDetailsData orderData = new OrderWithDetailsData { Id = id };
        OrderDetailDTO expectedDTO = new OrderDetailDTO { Id = id };

        SetupCacheMiss<OrderDetailDTO?>();

        orderRepoMock
            .Setup(r => r.GetByIdWithDetailsAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(orderData);

        mapperMock
            .Setup(m => m.Map<OrderDetailDTO>(orderData))
            .Returns(expectedDTO);

        OrderDetailDTO result = await orderService.GetByIdAsync(id);

        result.Should().NotBeNull();
        result.Id.Should().Be(id);
    }

    [Fact]
    public async Task GetByIdAsync_WhenUserIsNotOwnerAndNotAdmin_ThrowsForbiddenException()
    {
        Guid orderId = Guid.NewGuid();
        Guid orderOwnerId = Guid.NewGuid();
        Guid requestingUserId = Guid.NewGuid();

        OrderWithDetailsData orderData = new OrderWithDetailsData { Id = orderId, UserId = orderOwnerId };
        OrderDetailDTO dto = new OrderDetailDTO { Id = orderId, UserId = orderOwnerId };
        ClaimsPrincipal claims = new ClaimsPrincipal();

        SetupCacheMiss<OrderDetailDTO?>();

        orderRepoMock
            .Setup(r => r.GetByIdWithDetailsAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(orderData);

        mapperMock
            .Setup(m => m.Map<OrderDetailDTO>(orderData))
            .Returns(dto);

        userClaimsExtractorMock.Setup(e => e.GetUserId(claims)).Returns(requestingUserId);
        userClaimsExtractorMock.Setup(e => e.IsInRole(claims, "Admin")).Returns(false);
        userClaimsExtractorMock.Setup(e => e.IsInRole(claims, "Manager")).Returns(false);

        Func<Task> act = async () => await orderService.GetByIdAsync(orderId, claims);

        await act.Should().ThrowAsync<ForbiddenException>();
    }

    [Fact]
    public async Task UpdateStatusAsync_WhenStatusSetToDelivered_PublishesOrderDeliveredEmailEvent()
    {
        Guid id = Guid.NewGuid();
        Order order = new Order { Id = id, Status = OrderStatus.Processing };
        OrderDetailDTO detailDTO = new OrderDetailDTO
        {
            Id = id,
            Status = OrderStatus.Delivered,
            DeliveryDetail = new DeliveryDetailDTO { Email = "user@test.com" }
        };
        
        orderRepoMock
            .Setup(r => r.GetByIdAsync(id, default, null))
            .ReturnsAsync(order);
        SetupCacheMiss<OrderDetailDTO?>();

        orderRepoMock
            .Setup(r => r.GetByIdWithDetailsAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new OrderWithDetailsData { Id = id });

        mapperMock
            .Setup(m => m.Map<OrderDetailDTO>(It.IsAny<OrderWithDetailsData>()))
            .Returns(detailDTO);

        await orderService.UpdateStatusAsync(id, new OrderUpdateStatusDTO { Status = OrderStatus.Delivered });

        publishEndpointMock.Verify(p => p.Publish(
            It.IsAny<OrderDeliveredEmailEvent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateStatusAsync_WhenStatusSetToShipped_PublishesOrderShippedEmailEvent()
    {
        Guid id = Guid.NewGuid();
        Order order = new Order { Id = id, Status = OrderStatus.Processing };
        OrderDetailDTO detailDTO = new OrderDetailDTO
        {
            Id = id,
            Status = OrderStatus.Shipped,
            DeliveryDetail = new DeliveryDetailDTO { Email = "user@test.com" }
        };

        orderRepoMock
            .Setup(r => r.GetByIdAsync(id, default, null))
            .ReturnsAsync(order);

        SetupCacheMiss<OrderDetailDTO?>();

        orderRepoMock
            .Setup(r => r.GetByIdWithDetailsAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new OrderWithDetailsData { Id = id });

        mapperMock
            .Setup(m => m.Map<OrderDetailDTO>(It.IsAny<OrderWithDetailsData>()))
            .Returns(detailDTO);

        await orderService.UpdateStatusAsync(id, new OrderUpdateStatusDTO { Status = OrderStatus.Shipped });

        publishEndpointMock.Verify(p => p.Publish(
            It.IsAny<OrderShippedEmailEvent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateStatusAsync_WhenSaveSucceeds_InvalidatesBothCaches()
    {
        Guid id = Guid.NewGuid();
        Order order = new Order { Id = id };
        OrderDetailDTO detailDTO = new OrderDetailDTO { Id = id, Status = OrderStatus.Processing };

        orderRepoMock
            .Setup(r => r.GetByIdAsync(id, default, null))
            .ReturnsAsync(order);

        SetupCacheMiss<OrderDetailDTO?>();

        orderRepoMock
            .Setup(r => r.GetByIdWithDetailsAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new OrderWithDetailsData { Id = id });

        mapperMock
            .Setup(m => m.Map<OrderDetailDTO>(It.IsAny<OrderWithDetailsData>()))
            .Returns(detailDTO);

        await orderService.UpdateStatusAsync(id, new OrderUpdateStatusDTO { Status = OrderStatus.Processing });

        orderInvalidationMock.Verify(c => c.InvalidateAllAsync(), Times.Once);
        orderInvalidationMock.Verify(c => c.InvalidateByIdAsync(id), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WhenOrderNotFound_ThrowsNotFoundException()
    {
        Guid id = Guid.NewGuid();

        orderRepoMock
            .Setup(r => r.GetByIdAsync(id, default, null))
            .ReturnsAsync((Order?)null);

        Func<Task> act = async () => await orderService.DeleteAsync(id);

        await act.Should().ThrowAsync<NotFoundException>().WithMessage($"*{id}*");
    }

    [Fact]
    public async Task DeleteAsync_WhenOrderExists_DeletesAndInvalidatesCache()
    {
        Guid id = Guid.NewGuid();
        Order order = new Order { Id = id };

        orderRepoMock
            .Setup(r => r.GetByIdAsync(id, default, null))
            .ReturnsAsync(order);

        await orderService.DeleteAsync(id);

        orderRepoMock.Verify(r => r.DeleteAsync(id, default, null), Times.Once);
        unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Once);
        orderInvalidationMock.Verify(c => c.InvalidateByIdAsync(id), Times.Once);
        orderInvalidationMock.Verify(c => c.InvalidateAllAsync(), Times.Once);
    }

    [Fact]
    public async Task HandleOrderPaidEventAsync_PublishesOrderCreatedEventAndEmailEvent()
    {
        Guid orderId = Guid.NewGuid();
        Guid userId = Guid.NewGuid();
        OrderPaidEvent paidEvent = new OrderPaidEvent { OrderId = orderId };

        Order order = new Order { Id = orderId, Status = OrderStatus.AwaitingPayment };
        OrderDetailDTO detailDTO = new OrderDetailDTO
        {
            Id = orderId,
            UserId = userId,
            UserEmail = "user@test.com",
            Status = OrderStatus.Processing,
            Items = new List<OrderItemDTO>()
        };

        orderRepoMock
            .Setup(r => r.GetByIdAsync(orderId, default, null))
            .ReturnsAsync(order);

        SetupCacheMiss<OrderDetailDTO?>();

        orderRepoMock
            .Setup(r => r.GetByIdWithDetailsAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new OrderWithDetailsData { Id = orderId });

        mapperMock
            .Setup(m => m.Map<OrderDetailDTO>(It.IsAny<OrderWithDetailsData>()))
            .Returns(detailDTO);

        orderReservationRepoMock
            .Setup(r => r.GetByOrderIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<OrderReservation>());

        await orderService.HandleOrderPaidEventAsync(paidEvent);

        publishEndpointMock.Verify(p => p.Publish(It.IsAny<OrderCreatedEvent>(), It.IsAny<CancellationToken>()), Times.Once);
        publishEndpointMock.Verify(p => p.Publish(It.IsAny<OrderCreatedEmailEvent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleOrderPaidEventAsync_ClearsUserBasket()
    {
        Guid orderId = Guid.NewGuid();
        Guid userId = Guid.NewGuid();
        OrderPaidEvent paidEvent = new OrderPaidEvent { OrderId = orderId };

        Order order = new Order { Id = orderId };
        OrderDetailDTO detailDTO = new OrderDetailDTO
        {
            Id = orderId,
            UserId = userId,
            UserEmail = "user@test.com",
            Status = OrderStatus.Processing,
            Items = new List<OrderItemDTO>()
        };

        orderRepoMock
            .Setup(r => r.GetByIdAsync(orderId, default, null))
            .ReturnsAsync(order);

        SetupCacheMiss<OrderDetailDTO?>();

        orderRepoMock
            .Setup(r => r.GetByIdWithDetailsAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new OrderWithDetailsData { Id = orderId });

        mapperMock
            .Setup(m => m.Map<OrderDetailDTO>(It.IsAny<OrderWithDetailsData>()))
            .Returns(detailDTO);

        orderReservationRepoMock
            .Setup(r => r.GetByOrderIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<OrderReservation>());

        await orderService.HandleOrderPaidEventAsync(paidEvent);

        basketGrpcClientMock.Verify(c => c.ClearUserBasketAsync(userId), Times.Once);
    }
}