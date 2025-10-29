using Clothy.CatalogService.BLL.DTOs.BrandDTOs;
using Clothy.CatalogService.BLL.DTOs.ClotheDTOs;
using Clothy.CatalogService.BLL.DTOs.ClothingTypeDTOs;
using Clothy.CatalogService.BLL.DTOs.CollectionDTOs;
using Clothy.CatalogService.BLL.DTOs.ColorDTOs;
using Clothy.CatalogService.BLL.DTOs.MaterialDTOs;
using Clothy.CatalogService.BLL.DTOs.SizeDTOs;
using Clothy.CatalogService.BLL.DTOs.TagDTOs;

namespace Clothy.Aggregator.Clients
{
    public class CatalogClient
    {
        private HttpClient httpClient;
        private ILogger<CatalogClient> logger;

        public CatalogClient(HttpClient httpClient, ILogger<CatalogClient> logger)
        {
            this.httpClient = httpClient;
            this.logger = logger;
        }

        public async Task<List<BrandReadDTO>?> GetBrandsAsync(CancellationToken ct)
        {
            try
            {
                var response = await httpClient.GetAsync("/api/brands", ct);
                if (!response.IsSuccessStatusCode)
                {
                    logger.LogWarning("Failed to fetch brands. Status code: {StatusCode}", response.StatusCode);
                    return null;
                }
                return await response.Content.ReadFromJsonAsync<List<BrandReadDTO>>(cancellationToken: ct);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error fetching brands from CatalogService");
                return null;
            }
        }

        public async Task<List<ClothingTypeReadDTO>?> GetClothingTypesAsync(CancellationToken ct)
        {
            try
            {
                var response = await httpClient.GetAsync("/api/clothing-types", ct);
                if (!response.IsSuccessStatusCode)
                {
                    logger.LogWarning("Failed to fetch clothing types. Status code: {StatusCode}", response.StatusCode);
                    return null;
                }
                return await response.Content.ReadFromJsonAsync<List<ClothingTypeReadDTO>>(cancellationToken: ct);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error fetching clothing types from CatalogService");
                return null;
            }
        }

        public async Task<List<CollectionWithCountDTO>?> GetCollectionsAsync(CancellationToken ct)
        {
            try
            {
                var response = await httpClient.GetAsync("/api/collections/with-item-count", ct);
                if (!response.IsSuccessStatusCode)
                {
                    logger.LogWarning("Failed to fetch collections. Status code: {StatusCode}", response.StatusCode);
                    return null;
                }
                return await response.Content.ReadFromJsonAsync<List<CollectionWithCountDTO>>(cancellationToken: ct);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error fetching collections from CatalogService");
                return null;
            }
        }

        public async Task<List<ColorWithCountDTO>?> GetColorsAsync(CancellationToken ct)
        {
            try
            {
                var response = await httpClient.GetAsync("/api/colors/with-stock", ct);
                if (!response.IsSuccessStatusCode)
                {
                    logger.LogWarning("Failed to fetch colors. Status code: {StatusCode}", response.StatusCode);
                    return null;
                }
                return await response.Content.ReadFromJsonAsync<List<ColorWithCountDTO>>(cancellationToken: ct);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error fetching colors from CatalogService");
                return null;
            }
        }

        public async Task<List<MaterialWithCountDTO>?> GetMaterialsAsync(CancellationToken ct)
        {
            try
            {
                var response = await httpClient.GetAsync("/api/materials/with-clothe-count", ct);
                if (!response.IsSuccessStatusCode)
                {
                    logger.LogWarning("Failed to fetch materials. Status code: {StatusCode}", response.StatusCode);
                    return null;
                }
                return await response.Content.ReadFromJsonAsync<List<MaterialWithCountDTO>>(cancellationToken: ct);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error fetching materials from CatalogService");
                return null;
            }
        }

        public async Task<List<SizeReadDTO>?> GetSizesAsync(CancellationToken ct)
        {
            try
            {
                var response = await httpClient.GetAsync("/api/sizes", ct);
                if (!response.IsSuccessStatusCode)
                {
                    logger.LogWarning("Failed to fetch sizes. Status code: {StatusCode}", response.StatusCode);
                    return null;
                }
                return await response.Content.ReadFromJsonAsync<List<SizeReadDTO>>(cancellationToken: ct);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error fetching sizes from CatalogService");
                return null;
            }
        }

        public async Task<List<TagWithCountDTO>?> GetTagsAsync(CancellationToken ct)
        {
            try
            {
                var response = await httpClient.GetAsync("/api/tags/with-item-count", ct);
                if (!response.IsSuccessStatusCode)
                {
                    logger.LogWarning("Failed to fetch tags. Status code: {StatusCode}", response.StatusCode);
                    return null;
                }
                return await response.Content.ReadFromJsonAsync<List<TagWithCountDTO>>(cancellationToken: ct);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error fetching tags from CatalogService");
                return null;
            }
        }

        public async Task<ClotheDetailDTO?> GetClotheByIdAsync(Guid id, CancellationToken ct)
        {
            try
            {
                var response = await httpClient.GetAsync($"/api/clothes/{id}", ct);
                if (!response.IsSuccessStatusCode)
                {
                    logger.LogWarning("Failed to fetch clothe with id {Id}. Status code: {StatusCode}", id, response.StatusCode);
                    return null;
                }
                return await response.Content.ReadFromJsonAsync<ClotheDetailDTO>(cancellationToken: ct);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error fetching clothe by id {Id}", id);
                return null;
            }
        }

        public async Task<PriceRangeDTO?> GetPriceRangeAsync(CancellationToken ct)
        {
            try
            {
                var response = await httpClient.GetAsync("/api/clothes/pricerange", ct);
                if (!response.IsSuccessStatusCode)
                {
                    logger.LogWarning("Failed to fetch price range. Status code: {StatusCode}", response.StatusCode);
                    return null;
                }
                return await response.Content.ReadFromJsonAsync<PriceRangeDTO>(cancellationToken: ct);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error fetching price range from CatalogService");
                return null;
            }
        }
    }
}
