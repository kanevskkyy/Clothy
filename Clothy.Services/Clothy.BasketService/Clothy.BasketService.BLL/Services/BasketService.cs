using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Clothy.BasketService.BLL.DTOs;
using Clothy.BasketService.BLL.Services.Interfaces;
using Clothy.BasketService.DAL.Repositories.Interfaces;
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

        public BasketService(IBasketRepository basketRepository, IMapper mapper, IOrderItemValidatorGrpcClient orderItemValidatorGrpcClient)
        {
            this.orderItemValidatorGrpcClient = orderItemValidatorGrpcClient;
            this.basketRepository = basketRepository;
            this.mapper = mapper;
        }

        public async Task<BasketDTO> AddOrUpdateItemAsync(Guid userId, BasketItemCreateDTO itemDto)
        {
            try
            {
                ValidateOrderItemsResponse validateOrderItems = await orderItemValidatorGrpcClient.ValidateOrderItemsAsync(new List<OrderItemToValidate>
                {
                    new OrderItemToValidate
                    {
                        ClotheId = itemDto.ClotheId.ToString(),
                        SizeId = itemDto.SizeId.ToString(),
                        ColorId = itemDto.ColorId.ToString(),
                        Quantity = itemDto.Quantity
                    }
                });

                ValidateOrderItemResponse? validationResult = validateOrderItems.Results.FirstOrDefault();
                if (validationResult == null || !validationResult.IsValid) throw new ValidationFailedException(validationResult?.ErrorMessage ?? "Validation failed via gRPC");

                BasketItem basketItem = mapper.Map<BasketItem>(itemDto);

                await basketRepository.AddOrUpdateItemAsync(userId, basketItem);

                return await GetBasketAsync(userId) ?? new BasketDTO { UserId = userId, Items = new List<BasketItemDTO>() };
            }
            catch (ValidationFailedException)
            {
                throw;
            }
            catch (RpcException rpcEx)
            {
                throw new ValidationFailedException($"gRPC validation failed: {rpcEx.Status.Detail}");
            }
            catch (Exception ex)
            {
                throw new ValidationFailedException($"Failed to add/update basket item: {ex.Message}");
            }
        }

        public async Task ClearBasketAsync(Guid userId)
        {
            await basketRepository.ClearBasketAsync(userId);
        }

        public async Task<BasketDTO?> GetBasketAsync(Guid userId)
        {
            BasketList? basket = await basketRepository.GetBasketAsync(userId);
            if (basket == null || !basket.BasketItems.Any()) return null;

            List<OrderItemToValidate> itemsToValidate = basket.BasketItems.Select(item => new OrderItemToValidate
            {
                ClotheId = item.ClotheId.ToString(),
                SizeId = item.SizeId.ToString(),
                ColorId = item.ColorId.ToString(),
                Quantity = item.Quantity
            }).ToList();

            ValidateOrderItemsResponse validateResponse = await orderItemValidatorGrpcClient.ValidateOrderItemsAsync(itemsToValidate);

            BasketDTO basketDto = new BasketDTO
            {
                UserId = basket.UserId,
                Items = new List<BasketItemDTO>()
            };

            for (int i = 0; i < basket.BasketItems.Count; i++)
            {
                BasketItem item = basket.BasketItems[i];
                ValidateOrderItemResponse validation = validateResponse.Results[i];

                BasketItemDTO basketItemDto = new BasketItemDTO
                {
                    ClotheId = item.ClotheId,
                    SizeId = item.SizeId,
                    ColorId = item.ColorId,
                    Quantity = item.Quantity
                };

                basketItemDto.ClotheName = validation.ClotheName ?? "Unknown product";
                basketItemDto.ColorName = validation.ColorName ?? "";
                basketItemDto.SizeName = validation.SizeName ?? "";
                basketItemDto.MainPhoto = validation.MainPhotoUrl ?? "";
                basketItemDto.HexCode = validation.ColorHexCode ?? "";
                basketItemDto.ColorSlug = validation.ColorSlug ?? "";
                basketItemDto.ClotheSlug = validation.ClotheSlug ?? "";

                if (!string.IsNullOrEmpty(validation.Price) && decimal.TryParse(validation.Price, out decimal price))
                {
                    basketItemDto.Price = price;
                }
                else
                {
                    basketItemDto.Price = 0;
                }

                if (validation != null && validation.IsValid)
                {
                    basketItemDto.IsAvailable = true;
                    basketItemDto.ValidationMessage = null;
                }
                else
                {
                    basketItemDto.IsAvailable = false;
                    basketItemDto.ValidationMessage = validation?.ErrorMessage ?? "This item is no longer available";
                }

                basketDto.Items.Add(basketItemDto);
            }

            return basketDto;
        }

        public async Task RemoveItemAsync(Guid userId, Guid clotheId, Guid sizeId, Guid colorId)
        {
            await basketRepository.RemoveItemAsync(userId, clotheId, sizeId, colorId);
        }

        public async Task<BasketDTO> UpdateItemQuantityAsync(Guid userId, BasketItemUpdateDTO updateDto)
        {
            try
            {
                ValidateOrderItemsResponse validateOrderItems = await orderItemValidatorGrpcClient.ValidateOrderItemsAsync(new List<OrderItemToValidate>
                {
                    new OrderItemToValidate
                    {
                        ClotheId = updateDto.ClotheId.ToString(),
                        SizeId = updateDto.SizeId.ToString(),
                        ColorId = updateDto.ColorId.ToString(),
                        Quantity = updateDto.Quantity
                    }
                });

                ValidateOrderItemResponse? validationResult = validateOrderItems.Results.FirstOrDefault();
                if (validationResult == null || !validationResult.IsValid) throw new ValidationFailedException(validationResult?.ErrorMessage ?? "Validation failed via gRPC");

                await basketRepository.UpdateItemQuantityAsync(userId, updateDto.ClotheId, updateDto.SizeId, updateDto.ColorId, updateDto.Quantity);
                return await GetBasketAsync(userId) ?? new BasketDTO
                {
                    UserId = userId,
                    Items = new List<BasketItemDTO>()
                };
            }
            catch (RpcException rpcEx)
            {
                throw new ValidationFailedException($"gRPC validation failed: {rpcEx.Status.Detail}");
            }
            catch (Exception ex)
            {
                throw new ValidationFailedException($"Failed to update basket item quantity: {ex.Message}");
            }
        }
    }
}