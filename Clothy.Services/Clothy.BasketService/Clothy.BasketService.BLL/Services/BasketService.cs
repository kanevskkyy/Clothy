using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Clothy.BasketService.BLL.DTOs;
using Clothy.BasketService.BLL.Services.Interfaces;
using Clothy.BasketService.DAL.Repositories.Interfaces;
using Clothy.BasketService.gRPC.Client.Services.Interfaces;
using Clothy.BaskteService.Domain.Entities;
using Clothy.OrderService.gRPC.Client.Services.Interfaces;
using Clothy.Shared.Helpers.Exceptions;
using Grpc.Core;

namespace Clothy.BasketService.BLL.Services
{
    public class BasketService : IBasketService
    {
        private IBasketRepository basketRepository;
        private IMapper mapper;
        private IOrderItemValidatorGrpcClient orderItemValidatorGrpcClient;
        private IOrderHistoryGrpcClient orderHistoryGrpcClient;

        public BasketService(
            IBasketRepository basketRepository,
            IMapper mapper,
            IOrderItemValidatorGrpcClient orderItemValidatorGrpcClient,
            IOrderHistoryGrpcClient orderHistoryGrpcClient)
        {
            this.basketRepository = basketRepository;
            this.mapper = mapper;
            this.orderItemValidatorGrpcClient = orderItemValidatorGrpcClient;
            this.orderHistoryGrpcClient = orderHistoryGrpcClient;
        }

        public async Task<BasketDTO> AddOrUpdateItemAsync(Guid userId, BasketItemCreateDTO itemDto)
        {
            try
            {
                ValidateOrderItemsResponse validateResponse =
                    await orderItemValidatorGrpcClient.ValidateOrderItemsAsync(
                        new List<OrderItemToValidate>
                        {
                            new OrderItemToValidate
                            {
                                ClotheId = itemDto.ClotheId.ToString(),
                                SizeId = itemDto.SizeId.ToString(),
                                ColorId = itemDto.ColorId.ToString(),
                                Quantity = itemDto.Quantity
                            }
                        });

                ValidateOrderItemResponse? validationResult = validateResponse.Results.FirstOrDefault();
                if (validationResult == null || !validationResult.IsValid)
                    throw new ValidationFailedException(validationResult?.ErrorMessage ?? "Validation failed via gRPC");

                BasketItem basketItem = mapper.Map<BasketItem>(itemDto);
                await basketRepository.AddOrUpdateItemAsync(userId, basketItem);

                BasketList? basket = await basketRepository.GetBasketAsync(userId);
                if (basket == null || !basket.BasketItems.Any())
                    return new BasketDTO { UserId = userId, Items = new List<BasketItemDTO>() };

                return await BuildBasketDTOWithValidationAsync(userId, basket);
            }
            catch (ValidationFailedException) { throw; }
            catch (RpcException rpcEx)
            {
                throw new ValidationFailedException($"gRPC validation failed: {rpcEx.Status.Detail}");
            }
            catch (Exception ex)
            {
                throw new ValidationFailedException($"Failed to add/update basket item: {ex.Message}");
            }
        }

        public async Task<BasketDTO?> GetBasketAsync(Guid userId)
        {
            BasketList? basket = await basketRepository.GetBasketAsync(userId);
            if (basket == null || !basket.BasketItems.Any()) return null;

            return await BuildBasketDTOWithValidationAsync(userId, basket);
        }

        public async Task<BasketDTO> UpdateItemQuantityAsync(Guid userId, BasketItemUpdateDTO updateDto)
        {
            try
            {
                ValidateOrderItemsResponse validateResponse =
                    await orderItemValidatorGrpcClient.ValidateOrderItemsAsync(
                        new List<OrderItemToValidate>
                        {
                            new OrderItemToValidate
                            {
                                ClotheId = updateDto.ClotheId.ToString(),
                                SizeId = updateDto.SizeId.ToString(),
                                ColorId = updateDto.ColorId.ToString(),
                                Quantity = updateDto.Quantity
                            }
                        });

                ValidateOrderItemResponse? validationResult = validateResponse.Results.FirstOrDefault();
                if (validationResult == null || !validationResult.IsValid)
                    throw new ValidationFailedException(validationResult?.ErrorMessage ?? "Validation failed via gRPC");

                await basketRepository.UpdateItemQuantityAsync(userId, updateDto.ClotheId, updateDto.SizeId,
                    updateDto.ColorId, updateDto.Quantity);

                BasketList? basket = await basketRepository.GetBasketAsync(userId);
                if (basket == null || !basket.BasketItems.Any())
                    return new BasketDTO { UserId = userId, Items = new List<BasketItemDTO>() };

                return await BuildBasketDTOFromCachedValidation(userId, basket,
                    updateDto.ClotheId, updateDto.SizeId, updateDto.ColorId, validationResult);
            }
            catch (ValidationFailedException) { throw; }
            catch (RpcException rpcEx)
            {
                throw new ValidationFailedException($"gRPC validation failed: {rpcEx.Status.Detail}");
            }
            catch (Exception ex)
            {
                throw new ValidationFailedException($"Failed to update basket item quantity: {ex.Message}");
            }
        }

        public async Task RemoveItemAsync(Guid userId, Guid clotheId, Guid sizeId, Guid colorId)
        {
            await basketRepository.RemoveItemAsync(userId, clotheId, sizeId, colorId);
        }

        public async Task ClearBasketAsync(Guid userId)
        {
            await basketRepository.ClearBasketAsync(userId);
        }

        private async Task<BasketDTO> BuildBasketDTOWithValidationAsync(Guid userId, BasketList basket)
        {
            List<OrderItemToValidate> itemsToValidate = basket.BasketItems
                .Select(item => new OrderItemToValidate
                {
                    ClotheId = item.ClotheId.ToString(),
                    SizeId = item.SizeId.ToString(),
                    ColorId = item.ColorId.ToString(),
                    Quantity = item.Quantity
                }).ToList();

            ValidateOrderItemsResponse validateResponse =
                await orderItemValidatorGrpcClient.ValidateOrderItemsAsync(itemsToValidate);
            bool hasOrdered = await orderHistoryGrpcClient.HasUserAlreadyOrderedAsync(userId);

            BasketDTO basketDto = new BasketDTO
            {
                UserId = basket.UserId,
                IsFirstOrder = !hasOrdered,
                Items = new List<BasketItemDTO>()
            };

            for (int i = 0; i < basket.BasketItems.Count; i++)
            {
                BasketItem item = basket.BasketItems[i];
                ValidateOrderItemResponse validation = validateResponse.Results[i];
                basketDto.Items.Add(MapToBasketItemDTO(item, validation));
            }

            return basketDto;
        }

        private async Task<BasketDTO> BuildBasketDTOFromCachedValidation(Guid userId, BasketList basket,
            Guid updatedClotheId, Guid updatedSizeId, Guid updatedColorId, ValidateOrderItemResponse cachedValidation)
        {
            bool hasOrdered = await orderHistoryGrpcClient.HasUserAlreadyOrderedAsync(userId);

            BasketDTO basketDto = new BasketDTO
            {
                UserId = basket.UserId,
                IsFirstOrder = !hasOrdered,
                Items = new List<BasketItemDTO>()
            };

            foreach (BasketItem item in basket.BasketItems)
            {
                bool isUpdatedItem = item.ClotheId == updatedClotheId
                                     && item.SizeId == updatedSizeId
                                     && item.ColorId == updatedColorId;

                if (isUpdatedItem) basketDto.Items.Add(MapToBasketItemDTO(item, cachedValidation));
                else
                {
                    BasketItemDTO dto = mapper.Map<BasketItemDTO>(item);
                    dto.IsAvailable = true;
                    basketDto.Items.Add(dto);
                }
            }

            return basketDto;
        }

        private static BasketItemDTO MapToBasketItemDTO(BasketItem item, ValidateOrderItemResponse validation)
        {
            return new BasketItemDTO
            {
                ClotheId = item.ClotheId,
                SizeId = item.SizeId,
                ColorId = item.ColorId,
                Quantity = item.Quantity,
                ClotheName = validation.ClotheName ?? "Unknown product",
                ColorName = validation.ColorName ?? "",
                SizeName = validation.SizeName ?? "",
                MainPhoto = validation.MainPhotoUrl ?? "",
                HexCode = validation.ColorHexCode ?? "",
                ColorSlug = validation.ColorSlug ?? "",
                ClotheSlug = validation.ClotheSlug ?? "",
                Price = decimal.TryParse(validation.Price, out decimal price) ? price : 0,
                IsAvailable = validation.IsValid,
                ValidationMessage = validation.IsValid
                    ? null
                    : validation.ErrorMessage ?? "This item is no longer available"
            };
        }
    }
}