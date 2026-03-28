using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.CatalogService.BLL.DTOs.ClotheDTOs;
using Clothy.CatalogService.BLL.Interfaces;
using Clothy.CatalogService.DAL.UOW;
using Clothy.CatalogService.Domain.Entities;
using Clothy.Shared.Helpers.Exceptions;
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

            if (string.IsNullOrWhiteSpace(request.Slug))
            {
                logger.LogWarning("Received null or empty Slug");
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Slug cannot be empty"));
            }

            logger.LogInformation("Starting finding clothe with Slug: {Slug}", request.Slug);

            try
            {
                context.CancellationToken.ThrowIfCancellationRequested();
                ClotheDetailDTO clotheItem = await clotheService.GetDetailBySlugAsync(request.Slug, context.CancellationToken);

                ClotheDetailGrpcResponse clotheDetailGrpcResponse = new ClotheDetailGrpcResponse
                {
                    Id = clotheItem.Id.ToString(),
                    Name = clotheItem.Name,
                    Slug = clotheItem.Slug,
                    Description = clotheItem.Description,
                    Price = clotheItem.Price.ToString(),
                    Gender = clotheItem.Gender.ToString(),
                    Brand = new BrandDetailGrpcResponse
                    {
                        Id = clotheItem?.Brand?.Id.ToString(),
                        Name = clotheItem?.Brand?.Name,
                        Slug = clotheItem?.Brand?.Slug,
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
                    Name = tag.Name,
                    Slug = tag.Slug ?? string.Empty
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
                        Name = s?.Size?.Name
                    },
                    Color = new ColorGrpc
                    {
                        Id = s?.Color?.Id.ToString(),
                        HexCode = s?.Color?.HexCode,
                        Name = s?.Color?.Name,
                        Slug = s?.Color?.Slug
                    }
                }));

                logger.LogInformation("Successfully found clothe with Slug: {Slug}", request.Slug);

                return clotheDetailGrpcResponse;
            }
            catch (NotFoundException notFoundEx)
            {
                logger.LogWarning(notFoundEx, "Clothe with slug {Slug} not found", request.Slug);
                throw new RpcException(new Status(StatusCode.NotFound, notFoundEx.Message));
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
                logger.LogError(ex, "Unexpected error while fetching Clothe details for slug {Slug}", request.Slug);
                throw new RpcException(new Status(StatusCode.Internal, "Internal server error"));
            }
        }
    }
}