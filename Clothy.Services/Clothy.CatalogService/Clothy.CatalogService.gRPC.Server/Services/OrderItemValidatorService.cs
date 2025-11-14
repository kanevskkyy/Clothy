using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.CatalogService.DAL.UOW;
using Clothy.CatalogService.Domain.Entities;
using Grpc.Core;
using Microsoft.Extensions.Logging;

namespace Clothy.CatalogService.gRPC.Server.Services
{
    public class OrderItemValidatorService : OrderItemValidator.OrderItemValidatorBase
    {
        private IUnitOfWork unitOfWork;
        private ILogger<OrderItemValidatorService> logger;

        public OrderItemValidatorService(IUnitOfWork unitOfWork, ILogger<OrderItemValidatorService> logger)
        {
            this.unitOfWork = unitOfWork;
            this.logger = logger;
        }

        public override async Task<ValidateOrderItemsResponse> ValidateOrderItems(ValidateOrderItemsRequest request, ServerCallContext context)
        {
            if (request == null)
            {
                logger.LogWarning("Received null request");
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Request cannot be null"));
            }

            if (request.Items == null || request.Items.Count == 0)
            {
                logger.LogWarning("Received empty items list");
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Items list cannot be empty"));
            }

            logger.LogInformation("Starting validation for {ItemCount} order items", request.Items.Count);

            List<ValidateOrderItemResponse> results = new List<ValidateOrderItemResponse>();

            try
            {
                foreach (var item in request.Items)
                {
                    context.CancellationToken.ThrowIfCancellationRequested();

                    ValidateOrderItemResponse response = new ValidateOrderItemResponse();

                    try
                    {
                        if (!Guid.TryParse(item.ClotheId, out var clotheId))
                        {
                            response.IsValid = false;
                            response.ErrorMessage = $"Invalid ClotheId format: {item.ClotheId}";
                            results.Add(response);
                            continue;
                        }

                        if (!Guid.TryParse(item.ColorId, out var colorId))
                        {
                            response.IsValid = false;
                            response.ErrorMessage = $"Invalid ColorId format: {item.ColorId}";
                            results.Add(response);
                            continue;
                        }

                        if (!Guid.TryParse(item.SizeId, out var sizeId))
                        {
                            response.IsValid = false;
                            response.ErrorMessage = $"Invalid SizeId format: {item.SizeId}";
                            results.Add(response);
                            continue;
                        }

                        ClotheItem? clotheItem = await unitOfWork.ClotheItems.GetByIdAsync(clotheId, context.CancellationToken);
                        if (clotheItem == null)
                        {
                            response.IsValid = false;
                            response.ErrorMessage = $"Clothe with Id {item.ClotheId} not found";
                            logger.LogWarning("Clothe item not found: {ClotheId}", item.ClotheId);
                            results.Add(response);
                            continue;
                        }

                        Color? color = await unitOfWork.Colors.GetByIdAsync(colorId, context.CancellationToken);
                        if (color == null)
                        {
                            response.IsValid = false;
                            response.ErrorMessage = $"Color with Id {item.ColorId} not found";
                            logger.LogWarning("Color not found: {ColorId}", item.ColorId);
                            results.Add(response);
                            continue;
                        }

                        Size? size = await unitOfWork.Sizes.GetByIdAsync(sizeId, context.CancellationToken);
                        if (size == null)
                        {
                            response.IsValid = false;
                            response.ErrorMessage = $"Size with Id {item.SizeId} not found";
                            logger.LogWarning("Size not found: {SizeId}", item.SizeId);
                            results.Add(response);
                            continue;
                        }

                        ClothesStock? stock = await unitOfWork.ClothesStocks.GetByClotheColorSizeAsync(clotheId, colorId, sizeId, context.CancellationToken);

                        if (stock == null)
                        {
                            response.IsValid = false;
                            response.ErrorMessage = $"Combination of ClotheId: {item.ClotheId}, ColorId: {item.ColorId}, SizeId: {item.SizeId} does not exist in stock";
                            logger.LogWarning("Stock combo not found: ClotheId={ClotheId}, ColorId={ColorId}, SizeId={SizeId}", clotheId, colorId, sizeId);
                            results.Add(response);
                            continue;
                        }

                        if (stock.Quantity < item.Quantity)
                        {
                            response.IsValid = false;
                            response.ErrorMessage = $"Insufficient stock. Available: {stock.Quantity}, Requested: {item.Quantity}";
                            logger.LogWarning("Insufficient stock for ClotheId={ClotheId}, ColorId={ColorId}, SizeId={SizeId}. Available: {Available}, Requested: {Requested}", clotheId, colorId, sizeId, stock.Quantity, item.Quantity);
                            results.Add(response);
                            continue;
                        }

                        response.IsValid = true;
                        response.ErrorMessage = string.Empty;
                        response.ClotheName = clotheItem.Name;
                        response.Price = clotheItem.Price.ToString();
                        response.MainPhotoUrl = clotheItem.MainPhotoURL ?? string.Empty;
                        response.ColorHexCode = color.HexCode;
                        response.SizeName = size.Name;

                        logger.LogInformation("Successfully validated item: ClotheId={ClotheId}, ColorId={ColorId}, SizeId={SizeId}", clotheId, colorId, sizeId);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Database error while validating item. ClotheId: {ClotheId}, ColorId: {ColorId}, SizeId: {SizeId}", item.ClotheId, item.ColorId, item.SizeId);
                        throw new RpcException(new Status(StatusCode.Internal, "Database error occurred during validation"));
                    }

                    results.Add(response);
                }

                int validCount = results.Count(r => r.IsValid);
                logger.LogInformation("Validation completed. Valid: {ValidCount}/{TotalCount}", validCount, results.Count);

                return new ValidateOrderItemsResponse
                {
                    Results = { results }
                };
            }
            catch (OperationCanceledException)
            {
                logger.LogWarning("Validation operation was cancelled by client");
                throw new RpcException(new Status(StatusCode.Cancelled, "Operation was cancelled"));
            }
            catch (RpcException)
            {
                throw;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unexpected error during order items validation");
                throw new RpcException(new Status(StatusCode.Internal, "An internal error occurred during validation"));
            }
        }
    }
}