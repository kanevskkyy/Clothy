using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.Aggregator.Aggregate.Clients.Interfaces;
using Clothy.Aggregator.Aggregate.DTOs.Filters;
using Clothy.Shared.Cache.Interfaces;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using static ClotheFilterServiceGrpc;

namespace Clothy.Aggregator.Aggregate.Clients
{
    public class FilterGrpcClient : IFilterGrpcClient
    {
        private ClotheFilterServiceGrpcClient client;
        private ILogger<FilterGrpcClient> logger;
        private readonly IEntityCacheService cacheService;
        private static TimeSpan MEMORY_TTL_CACHE = TimeSpan.FromMinutes(5);
        private static TimeSpan REDIS_TTL_CACHE = TimeSpan.FromMinutes(30);
        private static string FILTER_CACHE_KEY = "filters:all";

        public FilterGrpcClient(ClotheFilterServiceGrpcClient grpcClient, ILogger<FilterGrpcClient> logger, IEntityCacheService entityCacheService)
        {
            this.cacheService = entityCacheService;
            this.client = grpcClient;
            this.logger = logger;
        }

        public async Task<ClotheFiltersDTO?> GetAllFiltersAsync(CancellationToken ct)
        {
            try
            {
                ClotheFiltersDTO? cachedFilters = await cacheService.GetOrSetAsync(
                FILTER_CACHE_KEY,
                async () =>
                {
                    FilterResponse gRPCresponse = await client.GetAllFiltersAsync(new Empty(), cancellationToken: ct);

                    ClotheFiltersDTO response = new ClotheFiltersDTO
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
                },
                memoryExpiration: MEMORY_TTL_CACHE,
                redisExpiration: REDIS_TTL_CACHE
                );

                return cachedFilters;
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
