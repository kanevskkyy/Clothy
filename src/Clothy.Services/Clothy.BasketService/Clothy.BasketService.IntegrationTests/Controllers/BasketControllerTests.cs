using System.Net;
using System.Net.Cache;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Clothy.BasketService.BLL.DTOs;
using Clothy.BasketService.IntegrationTests.Infrastructure;
using FluentAssertions;
using Moq;
using Xunit;

namespace Clothy.BasketService.IntegrationTests.Controllers;

public class BasketControllerTests : IClassFixture<BasketServiceWebApplicationFactory>
    {
        private HttpClient client;
        private BasketServiceWebApplicationFactory factory;

        private static JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters = { new JsonStringEnumConverter() }
        };

        public BasketControllerTests(BasketServiceWebApplicationFactory factory)
        {
            this.factory = factory;
            client = factory.CreateClient();

            factory.OrderItemValidatorMock.Reset();
            factory.OrderHistoryMock.Reset();

            factory.OrderItemValidatorMock
                .Setup(x => x.ValidateOrderItemsAsync(It.IsAny<List<OrderItemToValidate>>()))
                .ReturnsAsync((List<OrderItemToValidate> items) =>
                {
                    ValidateOrderItemsResponse response = new ValidateOrderItemsResponse();
                    response.Results.AddRange(items.Select(_ => new ValidateOrderItemResponse
                    {
                        IsValid = true,
                        ClotheName = "Test Clothe",
                        ColorName = "Black",
                        SizeName = "M",
                        MainPhotoUrl = "https://example.com/photo.jpg",
                        ColorHexCode = "#000000",
                        ColorSlug = "black",
                        ClotheSlug = "test-clothe",
                        Price = "499.99"
                    }));
                    return response;
                });

            factory.OrderHistoryMock
                .Setup(x => x.HasUserAlreadyOrderedAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);
        }

        [Fact]
        public async Task GetBasket_WithoutAuthentication_ShouldReturnUnauthorized()
        {
            HttpResponseMessage response = await client.GetAsync("/api/basket");

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task GetBasket_WhenBasketEmpty_ShouldReturnOkWithNull()
        {
            Guid userId = Guid.NewGuid();
            client.AddAuthorizationHeader(userId);

            HttpResponseMessage response = await client.GetAsync("/api/basket");

            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }

        [Fact]
        public async Task GetBasket_AfterAddingItem_ShouldReturnBasketWithItem()
        {
            Guid userId = Guid.NewGuid();
            client.AddAuthorizationHeader(userId);

            await AddItemToBasket(userId);

            HttpResponseMessage response = await client.GetAsync("/api/basket");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            BasketDTO? basket = await response.Content.ReadFromJsonAsync<BasketDTO>(JsonOptions);
            
            basket.Should().NotBeNull();
            basket!.Items.Should().HaveCountGreaterThanOrEqualTo(1);
        }

        [Fact]
        public async Task GetBasket_IsFirstOrder_ShouldBeTrue_WhenUserNeverOrdered()
        {
            Guid userId = Guid.NewGuid();
            client.AddAuthorizationHeader(userId);

            factory.OrderHistoryMock
                .Setup(x => x.HasUserAlreadyOrderedAsync(userId, new CancellationToken()))
                .ReturnsAsync(false);

            await AddItemToBasket(userId);

            HttpResponseMessage response = await client.GetAsync("/api/basket");
            BasketDTO? basket = await response.Content.ReadFromJsonAsync<BasketDTO>(JsonOptions);

            basket!.IsFirstOrder.Should().BeTrue();
        }

        [Fact]
        public async Task AddItem_WithValidData_ShouldReturnOkWithBasket()
        {
            Guid userId = Guid.NewGuid();
            client.AddAuthorizationHeader(userId);

            BasketItemCreateDTO dto = BuildItemCreateDto();

            HttpResponseMessage response = await client.PostAsJsonAsync("/api/basket/items", dto);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            BasketDTO? basket = await response.Content.ReadFromJsonAsync<BasketDTO>(JsonOptions);
            basket.Should().NotBeNull();
            basket!.Items.Should().HaveCount(1);
            basket.Items[0].ClotheName.Should().Be("Test Clothe");
        }

        [Fact]
        public async Task AddItem_WithoutAuthentication_ShouldReturnUnauthorized()
        {
            BasketItemCreateDTO dto = BuildItemCreateDto();

            HttpResponseMessage response = await client.PostAsJsonAsync("/api/basket/items", dto);

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task AddItem_WhenValidationFails_ShouldReturnBadRequest()
        {
            Guid userId = Guid.NewGuid();
            client.AddAuthorizationHeader(userId);

            factory.OrderItemValidatorMock
                .Setup(x => x.ValidateOrderItemsAsync(It.IsAny<List<OrderItemToValidate>>()))
                .ReturnsAsync(() =>
                {
                    ValidateOrderItemsResponse response = new ValidateOrderItemsResponse();
                    response.Results.Add(new ValidateOrderItemResponse
                    {
                        IsValid = false,
                        ErrorMessage = "Item out of stock"
                    });
                    return response;
                });

            BasketItemCreateDTO dto = BuildItemCreateDto();

            HttpResponseMessage response = await client.PostAsJsonAsync("/api/basket/items", dto);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task AddItem_SameTwice_ShouldUpdateQuantity()
        {
            Guid userId = Guid.NewGuid();
            client.AddAuthorizationHeader(userId);

            BasketItemCreateDTO dto = BuildItemCreateDto();

            await client.PostAsJsonAsync("/api/basket/items", dto);
            HttpResponseMessage response = await client.PostAsJsonAsync("/api/basket/items", dto);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            BasketDTO? basket = await response.Content.ReadFromJsonAsync<BasketDTO>(JsonOptions);
            basket!.Items.Should().HaveCount(1);
            basket.Items[0].Quantity.Should().BeGreaterThan(1);
        }

        [Fact]
        public async Task UpdateItemQuantity_WithValidData_ShouldReturnUpdatedBasket()
        {
            Guid userId = Guid.NewGuid();
            client.AddAuthorizationHeader(userId);

            BasketItemDTO item = await AddItemToBasket(userId);

            BasketItemUpdateDTO updateDto = new BasketItemUpdateDTO
            {
                ClotheId = item.ClotheId,
                SizeId = item.SizeId,
                ColorId = item.ColorId,
                Quantity = 5
            };

            HttpResponseMessage response = await client.PutAsJsonAsync("/api/basket/items", updateDto);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            BasketDTO? basket = await response.Content.ReadFromJsonAsync<BasketDTO>(JsonOptions);
            BasketItemDTO updatedItem = basket!.Items.First(i =>
                i.ClotheId == item.ClotheId &&
                i.SizeId == item.SizeId &&
                i.ColorId == item.ColorId);

            updatedItem.Quantity.Should().Be(5);
        }

        [Fact]
        public async Task RemoveItem_ShouldReturnNoContent()
        {
            Guid userId = Guid.NewGuid();
            client.AddAuthorizationHeader(userId);

            BasketItemDTO item = await AddItemToBasket(userId);

            HttpResponseMessage response = await client.DeleteAsync($"/api/basket/items/{item.ClotheId}/{item.SizeId}/{item.ColorId}");

            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }

        [Fact]
        public async Task RemoveItem_BasketShouldBeEmpty_AfterRemovingLastItem()
        {
            Guid userId = Guid.NewGuid();
            client.AddAuthorizationHeader(userId);

            BasketItemDTO item = await AddItemToBasket(userId);

            await client.DeleteAsync($"/api/basket/items/{item.ClotheId}/{item.SizeId}/{item.ColorId}");

            HttpResponseMessage getResponse = await client.GetAsync("/api/basket");
            getResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }

        [Fact]
        public async Task ClearBasket_ShouldReturnNoContent()
        {
            Guid userId = Guid.NewGuid();
            client.AddAuthorizationHeader(userId);

            await AddItemToBasket(userId);

            HttpResponseMessage response = await client.DeleteAsync("/api/basket");

            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }

        [Fact]
        public async Task ClearBasket_ShouldRemoveAllItems()
        {
            Guid userId = Guid.NewGuid();
            client.AddAuthorizationHeader(userId);

            await AddItemToBasket(userId);
            await AddItemToBasket(userId, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

            await client.DeleteAsync("/api/basket");

            HttpResponseMessage getResponse = await client.GetAsync("/api/basket");
            getResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }
        
        private static BasketItemCreateDTO BuildItemCreateDto(
            Guid? clotheId = null,
            Guid? sizeId = null,
            Guid? colorId = null) => new()
        {
            ClotheId = clotheId ?? Guid.NewGuid(),
            SizeId = sizeId ?? Guid.NewGuid(),
            ColorId = colorId ?? Guid.NewGuid(),
            Quantity = 1
        };

        private async Task<BasketItemDTO> AddItemToBasket(
            Guid userId,
            Guid? clotheId = null,
            Guid? sizeId = null,
            Guid? colorId = null)
        {
            HttpClient httpClient = factory.CreateClient();
            httpClient.AddAuthorizationHeader(userId);

            BasketItemCreateDTO dto = BuildItemCreateDto(clotheId, sizeId, colorId);
            HttpResponseMessage response = await httpClient.PostAsJsonAsync("/api/basket/items", dto);

            if (!response.IsSuccessStatusCode)
            {
                string error = await response.Content.ReadAsStringAsync();
                throw new Exception($"AddItemToBasket failed. Status: {response.StatusCode}. Body: {error}");
            }

            BasketDTO? basket = await response.Content.ReadFromJsonAsync<BasketDTO>(JsonOptions);
            return basket!.Items.Last();
        }
    }