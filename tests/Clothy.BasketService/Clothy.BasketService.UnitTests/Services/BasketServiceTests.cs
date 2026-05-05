using AutoMapper;
using Clothy.BasketService.BLL.DTOs;
using Clothy.BasketService.DAL.Repositories.Interfaces;
using Clothy.BasketService.gRPC.Client.Services.Interfaces;
using Clothy.BaskteService.Domain.Entities;
using Clothy.OrderService.gRPC.Client.Services.Interfaces;
using Clothy.Shared.Helpers.Exceptions;
using FluentAssertions;
using Grpc.Core;
using Moq;
using Xunit;

namespace Clothy.BasketService.UnitTests.Services;

public class BasketServiceTests
{
    private Mock<IBasketRepository> basketRepoMock;
    private Mock<IMapper> mapperMock;
    private Mock<IOrderItemValidatorGrpcClient> orderItemValidatorGrpcClientMock;
    private Mock<IOrderHistoryGrpcClient> orderHistoryGrpcClientMock;

    private BLL.Services.BasketService basketService;

    public BasketServiceTests()
    {
        basketRepoMock = new Mock<IBasketRepository>();
        mapperMock = new Mock<IMapper>();
        orderItemValidatorGrpcClientMock = new Mock<IOrderItemValidatorGrpcClient>();
        orderHistoryGrpcClientMock = new Mock<IOrderHistoryGrpcClient>();

        basketService = new BLL.Services.BasketService(
            basketRepoMock.Object,
            mapperMock.Object,
            orderItemValidatorGrpcClientMock.Object,
            orderHistoryGrpcClientMock.Object);
    }

    private BasketList BuildBasketList(Guid? userId = null, int itemCount = 1)
    {
        BasketList basket = new BasketList
        {
            UserId = userId ?? Guid.NewGuid(),
            BasketItems = new List<BasketItem>()
        };

        for (int i = 0; i < itemCount; i++)
        {
            basket.BasketItems.Add(new BasketItem
            {
                ClotheId = Guid.NewGuid(),
                SizeId = Guid.NewGuid(),
                ColorId = Guid.NewGuid(),
                Quantity = 1
            });
        }

        return basket;
    }

    private ValidateOrderItemsResponse BuildValidResponse(int count = 1)
    {
        ValidateOrderItemsResponse response = new ValidateOrderItemsResponse();
        for (int i = 0; i < count; i++)
        {
            response.Results.Add(new ValidateOrderItemResponse
            {
                IsValid = true,
                ClotheName = "Nike Hoodie",
                ColorName = "Red",
                SizeName = "M",
                Price = "99.99",
                MainPhotoUrl = "https://photo.jpg",
                ColorHexCode = "#FF0000",
                ColorSlug = "red",
                ClotheSlug = "nike-hoodie"
            });
        }
        return response;
    }

    private ValidateOrderItemsResponse BuildInvalidResponse(string errorMessage = "Out of stock")
    {
        ValidateOrderItemsResponse response = new ValidateOrderItemsResponse();
        response.Results.Add(new ValidateOrderItemResponse
        {
            IsValid = false,
            ErrorMessage = errorMessage
        });
        return response;
    }
    
    [Fact]
    public async Task GetBasketAsync_WhenBasketIsNull_ReturnsNull()
    {
        Guid userId = Guid.NewGuid();

        basketRepoMock
            .Setup(r => r.GetBasketAsync(userId))
            .ReturnsAsync((BasketList?)null);

        BasketDTO? result = await basketService.GetBasketAsync(userId);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetBasketAsync_WhenBasketHasNoItems_ReturnsNull()
    {
        Guid userId = Guid.NewGuid();

        basketRepoMock
            .Setup(r => r.GetBasketAsync(userId))
            .ReturnsAsync(new BasketList { UserId = userId, BasketItems = new List<BasketItem>() });

        BasketDTO? result = await basketService.GetBasketAsync(userId);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetBasketAsync_WhenBasketHasItems_ReturnsBasketDTOWithCorrectUserId()
    {
        BasketList basket = BuildBasketList();

        basketRepoMock.Setup(r => r.GetBasketAsync(basket.UserId)).ReturnsAsync(basket);
        orderItemValidatorGrpcClientMock
            .Setup(c => c.ValidateOrderItemsAsync(It.IsAny<List<OrderItemToValidate>>()))
            .ReturnsAsync(BuildValidResponse());
        orderHistoryGrpcClientMock
            .Setup(c => c.HasUserAlreadyOrderedAsync(basket.UserId, default))
            .ReturnsAsync(false);

        BasketDTO? result = await basketService.GetBasketAsync(basket.UserId);

        result.Should().NotBeNull();
        result!.UserId.Should().Be(basket.UserId);
    }

    [Fact]
    public async Task GetBasketAsync_WhenUserHasNotOrdered_IsFirstOrderIsTrue()
    {
        BasketList basket = BuildBasketList();

        basketRepoMock.Setup(r => r.GetBasketAsync(basket.UserId)).ReturnsAsync(basket);
        orderItemValidatorGrpcClientMock
            .Setup(c => c.ValidateOrderItemsAsync(It.IsAny<List<OrderItemToValidate>>()))
            .ReturnsAsync(BuildValidResponse());
        orderHistoryGrpcClientMock
            .Setup(c => c.HasUserAlreadyOrderedAsync(basket.UserId, default))
            .ReturnsAsync(false);

        BasketDTO? result = await basketService.GetBasketAsync(basket.UserId);

        result!.IsFirstOrder.Should().BeTrue();
    }
    
    [Fact]
    public async Task AddOrUpdateItemAsync_WhenValidationFails_ThrowsValidationFailedException()
    {
        Guid userId = Guid.NewGuid();
        BasketItemCreateDTO itemDto = new BasketItemCreateDTO
        {
            ClotheId = Guid.NewGuid(),
            SizeId = Guid.NewGuid(),
            ColorId = Guid.NewGuid(),
            Quantity = 1
        };

        orderItemValidatorGrpcClientMock
            .Setup(c => c.ValidateOrderItemsAsync(It.IsAny<List<OrderItemToValidate>>()))
            .ReturnsAsync(BuildInvalidResponse("Out of stock"));

        Func<Task> act = async () => await basketService.AddOrUpdateItemAsync(userId, itemDto);

        await act.Should().ThrowAsync<ValidationFailedException>().WithMessage("*Out of stock*");
    }

    [Fact]
    public async Task AddOrUpdateItemAsync_WhenRpcExceptionThrown_ThrowsValidationFailedException()
    {
        Guid userId = Guid.NewGuid();
        BasketItemCreateDTO itemDto = new BasketItemCreateDTO
        {
            ClotheId = Guid.NewGuid(),
            SizeId = Guid.NewGuid(),
            ColorId = Guid.NewGuid(),
            Quantity = 1
        };

        orderItemValidatorGrpcClientMock
            .Setup(c => c.ValidateOrderItemsAsync(It.IsAny<List<OrderItemToValidate>>()))
            .ThrowsAsync(new RpcException(new Status(StatusCode.Unavailable, "Service unavailable")));

        Func<Task> act = async () => await basketService.AddOrUpdateItemAsync(userId, itemDto);

        await act.Should().ThrowAsync<ValidationFailedException>().WithMessage("*gRPC validation failed*");
    }

    [Fact]
    public async Task AddOrUpdateItemAsync_WhenValidationSucceeds_CallsAddOrUpdateItemAsync()
    {
        Guid userId = Guid.NewGuid();
        BasketItemCreateDTO itemDto = new BasketItemCreateDTO
        {
            ClotheId = Guid.NewGuid(),
            SizeId = Guid.NewGuid(),
            ColorId = Guid.NewGuid(),
            Quantity = 1
        };
        BasketItem basketItem = new BasketItem
        {
            ClotheId = itemDto.ClotheId,
            SizeId = itemDto.SizeId,
            ColorId = itemDto.ColorId,
            Quantity = itemDto.Quantity
        };

        orderItemValidatorGrpcClientMock
            .Setup(c => c.ValidateOrderItemsAsync(It.IsAny<List<OrderItemToValidate>>()))
            .ReturnsAsync(BuildValidResponse());
        mapperMock.Setup(m => m.Map<BasketItem>(itemDto)).Returns(basketItem);
        basketRepoMock.Setup(r => r.GetBasketAsync(userId)).ReturnsAsync((BasketList?)null);

        await basketService.AddOrUpdateItemAsync(userId, itemDto);

        basketRepoMock.Verify(r => r.AddOrUpdateItemAsync(userId, basketItem), Times.Once);
    }

    [Fact]
    public async Task AddOrUpdateItemAsync_WhenBasketEmptyAfterAdd_ReturnsEmptyBasketDTO()
    {
        Guid userId = Guid.NewGuid();
        BasketItemCreateDTO itemDto = new BasketItemCreateDTO
        {
            ClotheId = Guid.NewGuid(),
            SizeId = Guid.NewGuid(),
            ColorId = Guid.NewGuid(),
            Quantity = 1
        };

        orderItemValidatorGrpcClientMock
            .Setup(c => c.ValidateOrderItemsAsync(It.IsAny<List<OrderItemToValidate>>()))
            .ReturnsAsync(BuildValidResponse());
        mapperMock.Setup(m => m.Map<BasketItem>(itemDto)).Returns(new BasketItem());
        basketRepoMock.Setup(r => r.GetBasketAsync(userId)).ReturnsAsync((BasketList?)null);

        BasketDTO result = await basketService.AddOrUpdateItemAsync(userId, itemDto);

        result.Should().NotBeNull();
        result.UserId.Should().Be(userId);
        result.Items.Should().BeEmpty();
    }
    
    [Fact]
    public async Task UpdateItemQuantityAsync_WhenValidationFails_ThrowsValidationFailedException()
    {
        Guid userId = Guid.NewGuid();
        BasketItemUpdateDTO updateDto = new BasketItemUpdateDTO
        {
            ClotheId = Guid.NewGuid(),
            SizeId = Guid.NewGuid(),
            ColorId = Guid.NewGuid(),
            Quantity = 2
        };

        orderItemValidatorGrpcClientMock
            .Setup(c => c.ValidateOrderItemsAsync(It.IsAny<List<OrderItemToValidate>>()))
            .ReturnsAsync(BuildInvalidResponse("Not enough stock"));

        Func<Task> act = async () => await basketService.UpdateItemQuantityAsync(userId, updateDto);

        await act.Should().ThrowAsync<ValidationFailedException>().WithMessage("*Not enough stock*");
        basketRepoMock.Verify(r => r.UpdateItemQuantityAsync(
            It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task UpdateItemQuantityAsync_WhenRpcExceptionThrown_ThrowsValidationFailedException()
    {
        Guid userId = Guid.NewGuid();
        BasketItemUpdateDTO updateDto = new BasketItemUpdateDTO
        {
            ClotheId = Guid.NewGuid(),
            SizeId = Guid.NewGuid(),
            ColorId = Guid.NewGuid(),
            Quantity = 2
        };

        orderItemValidatorGrpcClientMock
            .Setup(c => c.ValidateOrderItemsAsync(It.IsAny<List<OrderItemToValidate>>()))
            .ThrowsAsync(new RpcException(new Status(StatusCode.Unavailable, "Service unavailable")));

        Func<Task> act = async () => await basketService.UpdateItemQuantityAsync(userId, updateDto);

        await act.Should().ThrowAsync<ValidationFailedException>().WithMessage("*gRPC validation failed*");
    }

    [Fact]
    public async Task UpdateItemQuantityAsync_WhenValidationSucceeds_CallsUpdateItemQuantityAsync()
    {
        Guid userId = Guid.NewGuid();
        BasketItemUpdateDTO updateDto = new BasketItemUpdateDTO
        {
            ClotheId = Guid.NewGuid(),
            SizeId = Guid.NewGuid(),
            ColorId = Guid.NewGuid(),
            Quantity = 3
        };

        orderItemValidatorGrpcClientMock
            .Setup(c => c.ValidateOrderItemsAsync(It.IsAny<List<OrderItemToValidate>>()))
            .ReturnsAsync(BuildValidResponse());
        basketRepoMock
            .Setup(r => r.GetBasketAsync(userId))
            .ReturnsAsync((BasketList?)null);

        await basketService.UpdateItemQuantityAsync(userId, updateDto);

        basketRepoMock.Verify(r => r.UpdateItemQuantityAsync(
            userId, updateDto.ClotheId, updateDto.SizeId, updateDto.ColorId, updateDto.Quantity), Times.Once);
    }

    [Fact]
    public async Task UpdateItemQuantityAsync_WhenBasketEmptyAfterUpdate_ReturnsEmptyBasketDTO()
    {
        Guid userId = Guid.NewGuid();
        BasketItemUpdateDTO updateDto = new BasketItemUpdateDTO
        {
            ClotheId = Guid.NewGuid(),
            SizeId = Guid.NewGuid(),
            ColorId = Guid.NewGuid(),
            Quantity = 1
        };

        orderItemValidatorGrpcClientMock
            .Setup(c => c.ValidateOrderItemsAsync(It.IsAny<List<OrderItemToValidate>>()))
            .ReturnsAsync(BuildValidResponse());
        basketRepoMock.Setup(r => r.GetBasketAsync(userId)).ReturnsAsync((BasketList?)null);

        BasketDTO result = await basketService.UpdateItemQuantityAsync(userId, updateDto);

        result.Should().NotBeNull();
        result.UserId.Should().Be(userId);
        result.Items.Should().BeEmpty();
    }
    
    [Fact]
    public async Task RemoveItemAsync_CallsRepositoryWithCorrectParameters()
    {
        Guid userId = Guid.NewGuid();
        Guid clotheId = Guid.NewGuid();
        Guid sizeId = Guid.NewGuid();
        Guid colorId = Guid.NewGuid();

        await basketService.RemoveItemAsync(userId, clotheId, sizeId, colorId);

        basketRepoMock.Verify(r => r.RemoveItemAsync(userId, clotheId, sizeId, colorId), Times.Once);
    }
    
    [Fact]
    public async Task ClearBasketAsync_CallsRepositoryWithCorrectUserId()
    {
        Guid userId = Guid.NewGuid();

        await basketService.ClearBasketAsync(userId);

        basketRepoMock.Verify(r => r.ClearBasketAsync(userId), Times.Once);
    }
}