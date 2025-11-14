using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.CatalogService.DAL.UOW;
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

                    bool isValid = true;
                    string errorMessage = "";
                    try
                    {
                        if (!Guid.TryParse(item.ClotheId, out var clotheId) || await unitOfWork.ClotheItems.GetByIdAsync(clotheId, context.CancellationToken) == null)
                        {
                            isValid = false;
                            errorMessage = $"Invalid ClotheItemId: {item.ClotheId}. Cannot find in DB or invalid GUID format";
                            logger.LogWarning("Clothe item not found or invalid GUID format: {ClotheId}", item.ClotheId);
                        }
                        if (!Guid.TryParse(item.ColorId, out var colorId) || await unitOfWork.Colors.GetByIdAsync(colorId, context.CancellationToken) == null)
                        {
                            isValid = false;
                            errorMessage = $"Invalid ColorId: {item.ColorId}. Cannot find in DB or invalid GUID format";
                            logger.LogWarning("Color not found or invalid GUID format: {ColorId}", item.ColorId);
                        }
                        if (!Guid.TryParse(item.SizeId, out var sizeId) || await unitOfWork.Sizes.GetByIdAsync(sizeId, context.CancellationToken) == null)
                        {
                            isValid = false;
                            errorMessage = $"Invalid SizeId: {item.SizeId}. Cannot find in DB or invalid GUID format";
                            logger.LogWarning("Size not found or invalid GUID format: {SizeId}", item.SizeId);
                        }
                        bool comboExists = await unitOfWork.ClothesStocks.IsSizeAndColorAndClotheIdsExists(sizeId, colorId, clotheId, context.CancellationToken);

                        if (!comboExists)
                        {
                            isValid = false;
                            errorMessage = $"Combination of ClotheId: {item.ClotheId}, ColorId: {item.ColorId}, SizeId: {item.SizeId} does not exist in stock";
                            logger.LogWarning("Stock combo not found: ClotheId={ClotheId}, ColorId={ColorId}, SizeId={SizeId}", clotheId, colorId, sizeId);
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Database error while validating item. ClotheId: {ClotheId}, ColorId: {ColorId}, SizeId: {SizeId}", item.ClotheId, item.ColorId, item.SizeId);
                        throw new RpcException(new Status(StatusCode.Internal, "Database error occurred during validation"));
                    }
                    results.Add(new ValidateOrderItemResponse
                    {
                        IsValid = isValid,
                        ErrorMessage = errorMessage
                    });
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