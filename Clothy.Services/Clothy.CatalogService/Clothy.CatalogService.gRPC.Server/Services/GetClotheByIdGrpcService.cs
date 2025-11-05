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
    public class GetClotheByIdGrpcService : ClotheServiceGrpc.ClotheServiceGrpcBase
    {
        private IUnitOfWork unitOfWork;
        private ILogger<GetClotheByIdGrpcService> logger;

        public GetClotheByIdGrpcService(IUnitOfWork unitOfWork, ILogger<GetClotheByIdGrpcService> logger)
        {
            this.unitOfWork = unitOfWork;
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
                ClotheItem? clotheItem = await unitOfWork.ClotheItems.GetByIdWithDetailsAsync(clotheItemId, context.CancellationToken); 
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
                    MainPhotoUrl = clotheItem.MainPhotoURL,
                    Price = clotheItem.Price.ToString(),
                    Brand = new BrandDetailGrpcResponse
                    {
                        Id = clotheItem.BrandId.ToString(),
                        Name = clotheItem.Brand.Name,
                        Slug = clotheItem.Brand.Slug,
                        PhotoUrl = clotheItem.Brand.PhotoURL
                    },
                    ClothingType = new ClothingTypeGrpcResponse
                    {
                        Id = clotheItem.ClothyType.Id.ToString(),
                        Name = clotheItem.ClothyType.Name,
                        Slug = clotheItem.ClothyType.Slug
                    },
                    Collection = new CollectionGrpcResponse
                    {
                        Id = clotheItem.Collection.Id.ToString(),
                        Name = clotheItem.Collection.Name,
                        Slug = clotheItem.Collection.Slug,
                    }
                };
                clotheDetailGrpcResponse.AdditionalPhotos.AddRange(clotheItem.Photos.Select(photo => new AdditionalPhotoGrpcResponse
                {
                    Id = photo.Id.ToString(),
                    PhotoUrl = photo.PhotoURL,
                }));
                clotheDetailGrpcResponse.Tags.AddRange(clotheItem.ClotheTags.Select(tag => new TagGrpcResponse
                {
                    Id = tag.TagId.ToString(),
                    Name = tag.Tag.Name
                }));
                clotheDetailGrpcResponse.Materials.AddRange(clotheItem.ClotheMaterials.Select(materials => new MaterialWithPercentageGrpcResponse
                {
                    Id = materials.Material.Id.ToString(),
                    Name = materials.Material.Name,
                    Percentage = materials.Percentage
                }));
                clotheDetailGrpcResponse.Stocks.AddRange(clotheItem.Stocks.Select(s => new ClotheStockGrpcResponse
                {
                    Id = s.Id.ToString(),
                    Quantity = s.Quantity,
                    Size = new SizeGrpc
                    {
                        Id = s.Size.Id.ToString(),
                        Name = s.Size.Name
                    },
                    Color = new ColorGrpc
                    {
                        Id = s.Color.Id.ToString(),
                        HexCode = s.Color.HexCode
                    }
                }));
                logger.LogInformation("Successfully fetched Clothe details for Id: {ClotheId}", clotheItemId);

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
