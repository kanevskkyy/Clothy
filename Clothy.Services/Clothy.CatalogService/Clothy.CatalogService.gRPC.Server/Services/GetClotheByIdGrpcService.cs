using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.CatalogService.BLL.DTOs.ClotheDTOs;
using Clothy.CatalogService.BLL.Interfaces;
using Clothy.CatalogService.DAL.UOW;
using Clothy.CatalogService.Domain.Entities;
using Grpc.Core;
using Microsoft.Extensions.Logging;

namespace Clothy.CatalogService.gRPC.Server.Services
{
    public class GetClotheByIdGrpcService : ClotheServiceGrpc.ClotheServiceGrpcBase
    {
        private IClotheService clotheService;
        private ILogger<GetClotheByIdGrpcService> logger;

        public GetClotheByIdGrpcService(IClotheService clotheService, ILogger<GetClotheByIdGrpcService> logger)
        {
            this.clotheService = clotheService;
            this.logger = logger;
        }

        public override async Task<ClotheDetailGrpcResponse> GetClotheById(ClotheIdGrpcRequest request, ServerCallContext context)
        {
            if (request == null)
            {
                logger.LogWarning("Received null request");
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Request cannot be null"));
            }

            if (string.IsNullOrWhiteSpace(request.Id))
            {
                logger.LogWarning("Received null or empty ClotheItemId");
                throw new RpcException(new Status(StatusCode.InvalidArgument, "ClotheItemId cannot be empty"));
            }

            logger.LogInformation("Starting finding ClotheItemId {ClotheItemId}", request.Id);

            try
            {
                context.CancellationToken.ThrowIfCancellationRequested();
                if (!Guid.TryParse(request.Id, out Guid clotheItemId))
                {
                    logger.LogWarning("Clothe item ID invalid GUID format: {ClotheId}", clotheItemId);
                    throw new RpcException(new Status(StatusCode.Internal, $"Clothe item ID invalid GUID format: {clotheItemId}"));
                }
                ClotheDetailDTO? clotheItem = await clotheService.GetDetailByIdAsync(clotheItemId, context.CancellationToken);
                if (clotheItem == null)
                {
                    logger.LogWarning("Clothe with Id {ClotheId} not found", clotheItemId);
                    throw new RpcException(new Status(StatusCode.NotFound, $"Clothe with Id {clotheItemId} not found"));
                }
                ClotheDetailGrpcResponse clotheDetailGrpcResponse = new ClotheDetailGrpcResponse
                {
                    Id = clotheItem.Id.ToString(),
                    Name = clotheItem.Name,
                    Slug = clotheItem.Slug,
                    Description = clotheItem.Description,
                    Price = clotheItem.Price.ToString(),
                    Brand = new BrandDetailGrpcResponse
                    {
                        Id = clotheItem?.Brand?.Id.ToString(),
                        Name = clotheItem?.Brand?.Name,
                        Slug = clotheItem?.Brand?.Slug,
                        PhotoUrl = clotheItem?.Brand?.PhotoURL
                    },
                    ClothingType = new ClothingTypeGrpcResponse
                    {
                        Id = clotheItem?.ClothyType?.Id.ToString(),
                        Name = clotheItem?.ClothyType?.Name,
                        Slug = clotheItem?.ClothyType?.Slug
                    },
                    Collection = new CollectionGrpcResponse
                    {
                        Id = clotheItem?.Collection?.Id.ToString(),
                        Name = clotheItem?.Collection?.Name,
                        Slug = clotheItem?.Collection?.Slug,
                    },
                };

                if (clotheItem.OldPrice.HasValue) clotheDetailGrpcResponse.OldPrice = clotheItem.OldPrice.Value.ToString();
                if (clotheItem.DiscountPercent.HasValue) clotheDetailGrpcResponse.DiscountPercentage = clotheItem.DiscountPercent.Value;

                clotheDetailGrpcResponse.AdditionalPhotos.AddRange(clotheItem?.AdditionalPhotos.Select(photo => new AdditionalPhotoGrpcResponse
                {
                    Id = photo.Id.ToString(),
                    PhotoUrl = photo.PhotoURL,
                    ColorId = photo.ColorId?.ToString(),
                    IsMain = photo.IsMain
                }));
                clotheDetailGrpcResponse.Tags.AddRange(clotheItem?.Tags.Select(tag => new TagGrpcResponse
                {
                    Id = tag.Id.ToString(),
                    Name = tag.Name
                }));
                clotheDetailGrpcResponse.Materials.AddRange(clotheItem?.Materials.Select(materials => new MaterialWithPercentageGrpcResponse
                {
                    Id = materials.Id.ToString(),
                    Name = materials.Name,
                    Percentage = materials.Percentage
                }));
                clotheDetailGrpcResponse.Stocks.AddRange(clotheItem?.Stocks.Select(s => new ClotheStockGrpcResponse
                {
                    Id = s.StockId.ToString(),
                    Quantity = s.Quantity,
                    Size = new SizeGrpc
                    {
                        Id = s?.Size?.Id.ToString(),
                        Name = s?.Size?.Name,
                        Slug = s?.Size?.Slug
                    },
                    Color = new ColorGrpc
                    {
                        Id = s?.Color?.Id.ToString(),
                        HexCode = s?.Color?.HexCode,
                        Name = s?.Color?.Name,
                        Slug = s?.Color?.Slug
                    }
                }));
                logger.LogInformation("Successfully fetched Clothe details with ID: {ID}", request.Id);

                return clotheDetailGrpcResponse;

            }
            catch (RpcException)
            {
                throw;
            }
            catch (OperationCanceledException)
            {
                logger.LogWarning("Request was cancelled while fetching Clothe details");
                throw new RpcException(new Status(StatusCode.Cancelled, "Request was cancelled"));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unexpected error while fetching Clothe details for Id {ClotheId}", request.Id);
                throw new RpcException(new Status(StatusCode.Internal, "Internal server error"));
            }
        }
    }
}