using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Clothy.BasketService.DAL.Repositories.Interfaces;
using Clothy.BaskteService.Domain.Entities;
using Clothy.OrderService.gRPC.Client.Services.Interfaces;
using Grpc.Core;
using Microsoft.Extensions.Logging;

namespace Clothy.BasketService.gRPC.Server.Server
{
    public class BasketGrpcService : BasketGrpc.BasketGrpcBase
    {
        private IBasketRepository basketRepository;
        private ILogger<BasketGrpcService> logger;
        private IOrderItemValidatorGrpcClient orderItemValidatorGrpcClient;

        public BasketGrpcService(IBasketRepository basketRepository, IOrderItemValidatorGrpcClient orderItemValidatorGrpcClient, ILogger<BasketGrpcService> logger)
        {
            this.basketRepository = basketRepository;
            this.orderItemValidatorGrpcClient = orderItemValidatorGrpcClient;
            this.logger = logger;
        }

        public override async Task<GetUserBasketResponse> GetUserBasket(GetUserBasketRequest request, ServerCallContext context)
        {
            try
            {
                if (!Guid.TryParse(request.UserId, out Guid userId))
                {
                    logger.LogWarning("Invalid userId format: {UserId}", request.UserId);
                    throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid userId format"));
                }

                BasketList? basket = await basketRepository.GetBasketAsync(userId);
                if (basket == null || !basket.BasketItems.Any())
                {
                    logger.LogInformation("Basket is empty for user: {UserId}", userId);
                    return new GetUserBasketResponse
                    {
                        UserId = request.UserId
                    };
                }

                List<OrderItemToValidate> itemsToValidate = basket.BasketItems.Select(item => new OrderItemToValidate
                {
                    ClotheId = item.ClotheId.ToString(),
                    SizeId = item.SizeId.ToString(),
                    ColorId = item.ColorId.ToString(),
                    Quantity = item.Quantity
                }).ToList();

                ValidateOrderItemsResponse validateResponse = await orderItemValidatorGrpcClient.ValidateOrderItemsAsync(itemsToValidate);

                GetUserBasketResponse response = new GetUserBasketResponse
                {
                    UserId = request.UserId
                };

                int validItemsCount = 0;
                int invalidItemsCount = 0;

                for (int i = 0; i < basket.BasketItems.Count; i++)
                {
                    BasketItem item = basket.BasketItems[i];
                    ValidateOrderItemResponse validation = validateResponse.Results[i];

                    if (validation != null && validation.IsValid)
                    {
                        response.Items.Add(new BasketItemMessage
                        {
                            ClotheId = item.ClotheId.ToString(),
                            SizeId = item.SizeId.ToString(),
                            ColorId = item.ColorId.ToString(),
                            Quantity = item.Quantity
                        });
                        validItemsCount++;
                    }
                    else
                    {
                        invalidItemsCount++;
                        logger.LogWarning("Invalid basket item for user {UserId}: ClotheId={ClotheId}, SizeId={SizeId}, ColorId={ColorId}. Reason: {Reason}", userId, item.ClotheId, item.SizeId, item.ColorId, validation?.ErrorMessage ?? "Unknown");
                    }
                }

                logger.LogInformation("Retrieved basket for user: {UserId}. Valid items: {ValidCount}, Invalid items: {InvalidCount}", userId, validItemsCount, invalidItemsCount);
                return response;
            }
            catch (RpcException)
            {
                throw;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving basket for user: {UserId}", request.UserId);
                throw new RpcException(new Status(StatusCode.Internal, "An error occurred while retrieving the basket"));
            }
        }

        public override async Task<ClearUserBasketResponse> ClearUserBasket(ClearUserBasketRequest request, ServerCallContext context)
        {
            try
            {
                if (!Guid.TryParse(request.UserId, out Guid userId))
                {
                    logger.LogWarning("Invalid userId format: {UserId}", request.UserId);
                    throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid userId format"));
                }

                await basketRepository.ClearBasketAsync(userId);
                logger.LogInformation("Cleared basket for user: {UserId}", userId);

                return new ClearUserBasketResponse
                {
                    Success = true
                };
            }
            catch (RpcException)
            {
                throw;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error clearing basket for user: {UserId}", request.UserId);
                throw new RpcException(new Status(StatusCode.Internal, "An error occurred while clearing the basket"));
            }
        }
    }
}