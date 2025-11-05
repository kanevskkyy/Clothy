using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.Aggregator.Aggregate.Clients.Interfaces;
using Clothy.Aggregator.Aggregate.DTOs.Filters;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using static ClotheFilterServiceGrpc;

namespace Clothy.Aggregator.Aggregate.Clients
{
    public class FilterGrpcClient : IFilterGrpcClient
    {
        private ClotheFilterServiceGrpcClient client;
        private ILogger<FilterGrpcClient> logger;

        public FilterGrpcClient(ClotheFilterServiceGrpcClient grpcClient, ILogger<FilterGrpcClient> logger)
        {
            this.client = grpcClient;
            this.logger = logger;
        }

        public async Task<ClotheFiltersDTO?> GetAllFiltersAsync(CancellationToken ct)
        {
            try
            {
                FilterResponse gRPCresponse = await client.GetAllFiltersAsync(new Empty(), cancellationToken: ct);
                ClotheFiltersDTO response = new ClotheFiltersDTO()
                {
                    Brands = gRPCresponse.Brands.Select(brand => new BrandsGrpcResponse
                    {
                        Id = brand.Id,
                        Name = brand.Name,
                        Slug = brand.Slug,
                        PhotoUrl = brand.PhotoUrl
                    }).ToList(),

                    ClothingTypes = gRPCresponse.ClothingTypes.Select(type => new ClothingTypesGrpcResponse
                    {
                        Id = type.Id,
                        Name = type.Name,
                        Slug = type.Slug
                    }).ToList(),

                    Collections = gRPCresponse.Collections.Select(collection => new CollectionsGrpcResponse
                    {
                        Id = collection.Id,
                        Name = collection.Name,
                        Slug = collection.Slug,
                        ClotheItemCount = collection.ClotheItemCount
                    }).ToList(),

                    Colors = gRPCresponse.Colors.Select(color => new ColorsGrpcResponse
                    {
                        Id = color.Id,
                        HexCode = color.HexCode,
                        ClotheItemCount = color.ClotheItemCount
                    }).ToList(),

                    Materials = gRPCresponse.Materials.Select(material => new MaterialsGrpcResponse
                    {
                        Id = material.Id,
                        Name = material.Name,
                        ClotheItemCount = material.ClotheItemCount
                    }).ToList(),

                    Sizes = gRPCresponse.Sizes.Select(size => new SizesGrpcResponse
                    {
                        Id = size.Id,
                        Name = size.Name
                    }).ToList(),

                    Tags = gRPCresponse.Tags.Select(tag => new TagsGrpcResponse
                    {
                        Id = tag.Id,
                        Name = tag.Name,
                        ClotheItemCount = tag.ClotheItemCount
                    }).ToList(),

                    PriceRange = new PriceGrpcResponse
                    {
                        MinPrice = gRPCresponse.PriceRange.MinPrice,
                        MaxPrice = gRPCresponse.PriceRange.MaxPrice
                    }
                };
                return response;
            }
            catch (RpcException ex)
            {
                logger.LogError(ex, "gRPC error while fetching filters from CatalogService");
                return null;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unexpected error while fetching filters from CatalogService");
                return null;
            }
        }
    }
}
